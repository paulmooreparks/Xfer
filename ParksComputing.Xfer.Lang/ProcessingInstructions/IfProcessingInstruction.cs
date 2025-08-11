using System;
using System.Threading;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;
using ParksComputing.Xfer.Lang.Scripting;
using ParksComputing.Xfer.Lang.Scripting.Logical;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions;

/// <summary>
/// Exception thrown when a conditional element should not be added to the document.
/// This signals to the parser that the element should be skipped.
/// </summary>
/// <summary>
/// Exception thrown when a conditional element fails its condition and should be skipped.
/// The parser catches this to avoid adding the element to the document without treating it as a fatal error.
/// </summary>
public class ConditionalElementException : Exception {
    /// <summary>
    /// Creates a new <see cref="ConditionalElementException"/>.
    /// </summary>
    /// <param name="message">Describes the element condition failure.</param>
    public ConditionalElementException(string message) : base(message) { }
}

/// <summary>
/// Processing instruction for conditional execution based on expressions.
/// Evaluates a condition and conditionally affects the target element based on the result.
/// Demonstrates the hybrid approach where processing instructions delegate to reusable scripting operators.
/// </summary>
/// <example>
/// Basic usage with defined check:
/// <code>
/// &lt;! if defined &lt;|DEBUG_MODE|&gt; !&gt;
/// debug-settings: { verbose: true }
/// </code>
///
/// Equality comparison:
/// <code>
/// &lt;! if eq["Windows" &lt;|PLATFORM|&gt;] !&gt;
/// windows-config: "value"
/// </code>
///
/// Complex expression (when implemented):
/// <code>
/// &lt;! if and[defined(&lt;|DEBUG|&gt;) eq["verbose" &lt;|LOG_LEVEL|&gt;]] !&gt;
/// verbose-logging: true
/// </code>
/// </example>
public class IfProcessingInstruction : ProcessingInstruction {
    /// <summary>
    /// The keyword identifier for this processing instruction.
    /// </summary>
    public const string Keyword = "if";

    /// <summary>
    /// The condition expression to evaluate.
    /// </summary>
    public Element ConditionExpression { get; private set; }

    /// <summary>
    /// The result of the condition evaluation - true if the condition is met, false otherwise.
    /// </summary>
    public bool ConditionMet { get; private set; }

    /// <summary>
    /// True when the operator name in the condition expression was unknown and treated as a no-op.
    /// Used to preserve the PI in serialization for user visibility.
    /// </summary>
    public bool UnknownOperator { get; private set; }

    /// <summary>
    /// The scripting engine used for evaluating the condition (cached for performance).
    /// </summary>
    private static ScriptingEngine? _scriptingEngine;

    /// <summary>
    /// Deterministic incremental identifier for debug correlation across creation, application, and final document state.
    /// </summary>
    private static int _nextId = 0;

    /// <summary>
    /// Public identifier (debug only usage) to correlate logs. Stable for lifetime of the PI instance.
    /// </summary>
    public int DebugId { get; }

    /// <summary>
    /// Snapshot of ConditionMet at the exact moment ElementHandler was invoked. Used to detect
    /// post-application mutation or evaluation/application divergence in intermittent failures.
    /// Null until ElementHandler runs for the first time.
    /// </summary>
    public bool? AppliedConditionMetSnapshot { get; private set; }

    /// <summary>
    /// Tracks whether ElementHandler executed (even if it later threw for suppression) to distinguish
    /// between "never applied" and "applied then mutated" scenarios.
    /// </summary>
    public bool ElementHandlerInvoked { get; private set; }

    /// <summary>
    /// Tracks whether the ProcessingInstructionHandler evaluation has executed. Intermittent test
    /// failures showed cases where ConditionMet remained at the default (false) yet the target
    /// element was still added, indicating the evaluation path may have been skipped (e.g. if the
    /// PI construction path changed or a future refactor omits the explicit handler call). To make
    /// the logic robust against any missed invocation, ElementHandler will lazily force evaluation
    /// when this is false before applying suppression logic.
    /// </summary>
    private bool _evaluated;

    /// <summary>
    /// Gets or creates the scripting engine for evaluating conditions.
    /// This is cached to avoid creating multiple engines for repeated evaluations.
    /// </summary>
    private static ScriptingEngine GetScriptingEngine() {
        if (_scriptingEngine == null) {
            var context = new ScriptingContext();
            _scriptingEngine = new ScriptingEngine(context);
        }
        return _scriptingEngine;
    }

    /// <summary>
    /// INTERNAL TEST HOOK: resets cached scripting engine to force clean context.
    /// Marked internal so production code does not casually invoke it; exposed to tests via InternalsVisibleTo if configured.
    /// </summary>
    internal static void __ResetCachedEngine_ForTests() {
        _scriptingEngine = null;
    }

    private readonly Services.Parser? _parser; // For emitting warnings

    /// <summary>
    /// Initializes a new instance of the <see cref="IfProcessingInstruction"/> class.
    /// The condition expression is stored and evaluated later during <see cref="ProcessingInstructionHandler"/>.
    /// </summary>
    /// <param name="conditionExpression">Expression whose truthiness determines whether the next element is included.</param>
    /// <param name="parser">Optional parser used for emitting warnings (e.g. unknown operator).</param>
    public IfProcessingInstruction(Element conditionExpression, Services.Parser? parser = null) : base(conditionExpression, Keyword) {
        ConditionExpression = conditionExpression ?? throw new ArgumentNullException(nameof(conditionExpression));
        _parser = parser;
    DebugId = Interlocked.Increment(ref _nextId);
    // Universal policy: all if PIs are stripped from serialization regardless of outcome.
    // Setting this in the ctor guarantees suppression even if future parser code paths
    // forget to toggle SuppressSerialization after creation.
    SuppressSerialization = true;
#if DEBUG
    Console.WriteLine($"[IF][ID={DebugId}][CTOR] Created IfProcessingInstruction with condition='{ConditionExpression}'");
#endif
    }

    /// <summary>
    /// Handles the processing of the conditional expression evaluation.
    /// This delegates to the IfOperator in the Scripting namespace for consistent logic
    /// while providing the Processing Instruction interface for document-level usage.
    /// </summary>
    public override void ProcessingInstructionHandler() {
        try {
            // Use the IfOperator for evaluation - this demonstrates the hybrid approach
            // where PIs delegate to reusable scripting operators
            var scriptingEngine = GetScriptingEngine();

            // For the if PI, we evaluate the condition and store the result
            // The actual conditional behavior is handled in ElementHandler
            ConditionMet = EvaluateCondition(ConditionExpression, scriptingEngine);
            _evaluated = true; // mark evaluation complete
            // Always emit evaluation snapshot (Release + Debug) for intermittent suppression diagnostics.
            Console.WriteLine($"[IF][ID={DebugId}][EVAL][v2] met={ConditionMet} unknownOp={UnknownOperator} expr='{ConditionExpression}'");
        }
        catch (Exception) {
            // If any error occurs during evaluation, consider the condition false
            ConditionMet = false;
            _evaluated = true;
            Console.WriteLine($"[IF][ID={DebugId}][EVAL][v2] exception -> force met=false expr='{ConditionExpression}'");
        }
    }

    /// <summary>
    /// Handles element processing based on the conditional evaluation result.
    /// If the condition is met, the element is processed normally.
    /// If the condition is not met, we signal that the element should not be added to the document.
    /// </summary>
    /// <param name="element">The target element to conditionally process.</param>
    public override void ElementHandler(Element element) {
        if (element == null) {
            return;
        }
    // Defensive: ensure evaluation happened even if ProcessingInstructionHandler was not invoked.
    if (!_evaluated) {
    Console.WriteLine($"[IF][ID={DebugId}][APPLY][LATE-EVAL][v2] Detected unevaluated PI - forcing evaluation before applying to {element.GetType().Name}");
        ProcessingInstructionHandler();
    }
    ElementHandlerInvoked = true;
    AppliedConditionMetSnapshot = ConditionMet; // snapshot BEFORE branch for post-mortem
    // Always log in Release too (minimal text) so failing CI artifacts contain correlation data.
    Console.WriteLine($"[IF][ID={DebugId}][APPLY] snapshotMet={AppliedConditionMetSnapshot} targetType={element.GetType().Name}");

    if (!ConditionMet) {
#if DEBUG
        Console.WriteLine($"[IF][ID={DebugId}][APPLY] conditionMet=false -> THROW suppress");
#endif
        // Signal removal via exception so parser catch blocks handle skipping (including root-handling logic there).
        throw new ConditionalElementException("Element condition not met - should not be added to document");
    }
    #if DEBUG
    Console.WriteLine($"[IF][ID={DebugId}][APPLY] conditionMet=true -> allow");
    #endif
    }

    /// <summary>
    /// Removes an element from its parent container.
    /// Handles different parent types (ObjectElement, ArrayElement, TupleElement).
    /// </summary>
    /// <param name="element">The element to remove from its parent.</param>
    private static void RemoveFromParent(Element element) {
        if (element.Parent == null) {
            Console.WriteLine($"[DEBUG] Element has no parent, cannot remove");
            return;
        }

        Console.WriteLine($"[DEBUG] Attempting to remove {element.GetType().Name} from {element.Parent.GetType().Name}");

        switch (element.Parent) {
            case ObjectElement objectParent:
                // For objects, we need to find the key that contains this element
                if (element is KeyValuePairElement kvp) {
                    bool removed = objectParent.Remove(kvp.Key);
                    Console.WriteLine($"[DEBUG] Removed from ObjectElement: {removed}");
                }
                break;

            case ArrayElement arrayParent:
                bool removedFromArray = arrayParent.Remove(element);
                Console.WriteLine($"[DEBUG] Removed from ArrayElement: {removedFromArray}");
                break;

            case TupleElement tupleParent:
                bool removedFromTuple = tupleParent.Remove(element);
                Console.WriteLine($"[DEBUG] Removed from TupleElement: {removedFromTuple}");
                break;

            case ListElement listParent:
                // Generic list element (covers other collection types)
                bool removedFromList = listParent.Remove(element);
                Console.WriteLine($"[DEBUG] Removed from ListElement: {removedFromList}");
                break;

            default:
                // For other parent types, try to remove from Children collection
                if (element.Parent.Children != null) {
                    bool removedFromChildren = element.Parent.Children.Remove(element);
                    if (removedFromChildren) {
                        element.Parent = null;
                    }
                    Console.WriteLine($"[DEBUG] Removed from Children collection: {removedFromChildren}");
                }
                break;
        }

        Console.WriteLine($"[DEBUG] After removal attempt, parent type: {element.Parent?.GetType().Name ?? "null"}");
    }

    /// <summary>
    /// Evaluates the condition expression using the appropriate method.
    /// This method determines the best way to evaluate the condition based on its type.
    /// </summary>
    /// <param name="conditionExpression">The condition expression to evaluate.</param>
    /// <param name="scriptingEngine">The scripting engine for evaluation.</param>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    private bool EvaluateCondition(Element conditionExpression, ScriptingEngine scriptingEngine) {
        // Support operator expressed as a key/value pair: op[args...]
        // e.g. eq["Linux" "<|PLATFORM|>"] parsed as KeyValuePairElement(key='eq', value = ArrayElement)
        if (conditionExpression is KeyValuePairElement kvpExp && kvpExp.Key is not null) {
            var opName = kvpExp.Key.Trim();
            if (!string.IsNullOrEmpty(opName)) {
                // Defensive: ensure built-ins are registered (idempotent) before deciding unknown.
                // This avoids a partial-registration race leading to misclassification as unknown.
                try {
                    OperatorRegistry.RegisterBuiltInOperators();
                } catch { /* best effort */ }

                bool known = IsKnownOperator(opName);
#if DEBUG
                #if DEBUG
                Console.WriteLine($"[TRACE-IF] Evaluating operator KVP op='{opName}' known={known} rawValue='{kvpExp.Value}'");
                #endif
#endif
                if (known) {
                    try {
                        // Gather arguments from value element
                        var valueElem = kvpExp.Value;
                        Element[] args;
                        if (valueElem is CollectionElement coll) {
#if DEBUG
                            #if DEBUG
                            Console.WriteLine($"[TRACE-IF] Operator '{opName}' argCount={coll.Count}");
                            #endif
#endif
                            args = new Element[coll.Count];
                            for (int i = 0; i < coll.Count; i++) {
                                var arg = coll.GetElementAt(i);
                                if (arg != null) {
                                    args[i] = arg; // assign positional arg
#if DEBUG
                                    #if DEBUG
                                    Console.WriteLine($"[TRACE-IF]  arg[{i}]='{arg}' type={arg.GetType().Name}");
                                    #endif
#endif
                                }
                            }
                        } else {
                            args = new[] { valueElem };
                        }
                        var result = scriptingEngine.Evaluate(opName, args);
#if DEBUG
                        #if DEBUG
                        Console.WriteLine($"[TRACE-IF] Operator '{opName}' evaluation result={result ?? "<null>"}");
                        #endif
#endif
                        return ConvertToBoolean(result);
                    }
                    catch (Exception ex) {
                        // Evaluation failure -> treat as false (do NOT mark unknown; operator existed but failed)
                        Console.WriteLine($"[DEBUG] Operator '{opName}' evaluation exception: {ex.GetType().Name}: {ex.Message}");
                        return false;
                    }
                }

                // Unknown operator: emit warning and treat as *no-op* (return true so target is preserved, PI remains)
                _parser?.AddWarning(WarningType.UnknownConditionalOperator, $"Unknown conditional operator '{opName}' in if PI (treated as no-op)");
                UnknownOperator = true;
                return true;
            }
        }

        // Direct dereference element: resolve binding value (if available) and evaluate its truthiness.
        if (conditionExpression is ReferenceElement derefElem) {
            if (_parser != null && _parser.TryResolveBinding(derefElem.Value, out var bound)) {
                // Use the bound element's parsed value for truthiness
#if DEBUG
                #if DEBUG
                Console.WriteLine($"[TRACE-IF] Dereference '_{derefElem.Value}' resolved to element='{bound}' parsedValue='{bound?.ParsedValue ?? "<null>"}' type={bound?.GetType().Name}");
                #endif
#endif
                return ConvertToBoolean(bound?.ParsedValue);
            }
            // Unresolved dereference counts as false
#if DEBUG
            #if DEBUG
            Console.WriteLine($"[TRACE-IF] Dereference '_{derefElem.Value}' unresolved -> false");
            #endif
#endif
            return false;
        }

        // If the condition is a simple, already-resolved element (literal, previously bound value, etc.)
        // evaluate its intrinsic truthiness directly instead of performing an implicit "defined" check.
        // Users can still request an explicit defined test via the 'defined' operator: <! if defined _name !>
        if (IsSimpleElement(conditionExpression)) {
            return ConvertToBoolean(conditionExpression.ParsedValue);
        }

        // If the condition is a collection that looks like an operator expression, evaluate it as such
        if (IsOperatorExpression(conditionExpression)) {
            return EvaluateOperatorExpression(conditionExpression, scriptingEngine);
        }

        // For other element types, use general truthiness evaluation
        var resolvedValue = ResolveElementValue(conditionExpression);
        return ConvertToBoolean(resolvedValue);
    }

    /// <summary>
    /// Determines if an element is a simple element (not a collection or operator expression).
    /// </summary>
    /// <param name="element">The element to check.</param>
    /// <returns>True if the element is simple; otherwise, false.</returns>
    private bool IsSimpleElement(Element element) {
        return element is not CollectionElement;
    }

    /// <summary>
    /// Determines if an element represents an operator expression.
    /// </summary>
    /// <param name="element">The element to check.</param>
    /// <returns>True if the element represents an operator expression; otherwise, false.</returns>
    private bool IsOperatorExpression(Element element) {
        if (element is not CollectionElement collection || collection.Count < 2) {
            return false;
        }

        // Check if the first element looks like an operator name
        var firstElement = collection.GetElementAt(0);
        if (firstElement is TextElement textElement) {
            var operatorName = textElement.Value?.Trim();
            return !string.IsNullOrEmpty(operatorName) && IsKnownOperator(operatorName);
        }

        return false;
    }

    /// <summary>
    /// Checks if a string represents a known operator name by querying the global operator registry.
    /// </summary>
    /// <param name="operatorName">The potential operator name to check.</param>
    /// <returns>True if the name represents a known operator; otherwise, false.</returns>
    private bool IsKnownOperator(string operatorName) {
        // Ensure built-in operators are registered at least once (defensive: some manual test runs
        // may invoke operator detection before any ScriptingEngine instance has been created).
        // If the registry is empty, register built-ins lazily.
        if (OperatorRegistry.GetRegisteredOperatorNames().Count == 0) {
            OperatorRegistry.RegisterBuiltInOperators();
        }
        return OperatorRegistry.IsOperatorRegistered(operatorName);
    }

    /// <summary>
    /// Evaluates an operator expression by delegating to the appropriate operator.
    /// </summary>
    /// <param name="element">The collection element containing the operator expression.</param>
    /// <param name="scriptingEngine">The scripting engine for evaluation.</param>
    /// <returns>The boolean result of the operator evaluation.</returns>
    private bool EvaluateOperatorExpression(Element element, ScriptingEngine scriptingEngine) {
        if (element is not CollectionElement collection || collection.Count < 2) {
            return false;
        }

        try {
            // Extract operator name
            var operatorNameElement = collection.GetElementAt(0);
            if (operatorNameElement is not TextElement textElement) {
                return false;
            }

            var operatorName = textElement.Value?.Trim();
            if (string.IsNullOrEmpty(operatorName)) {
                return false;
            }

            // Extract arguments (skip the first element which is the operator name)
            var operatorArguments = new Element[collection.Count - 1];
            for (int i = 1; i < collection.Count; i++) {
                var argElement = collection.GetElementAt(i);
                if (argElement != null) {
                    operatorArguments[i - 1] = argElement;
                }
            }

            // Evaluate using the scripting engine
            var result = scriptingEngine.Evaluate(operatorName, operatorArguments);

            // Convert result to boolean
            return ConvertToBoolean(result);
        }
        catch {
            // If evaluation fails, consider it false
            return false;
        }
    }

    /// <summary>
    /// Resolves an element to its actual value for truthiness evaluation.
    /// </summary>
    /// <param name="element">The element to resolve.</param>
    /// <returns>The resolved value of the element.</returns>
    private object? ResolveElementValue(Element element) {
        return element?.ParsedValue;
    }

    /// <summary>
    /// Converts a value to boolean using consistent truthiness rules.
    /// </summary>
    /// <param name="value">The value to convert to boolean.</param>
    /// <returns>The boolean representation of the value.</returns>
    private bool ConvertToBoolean(object? value) {
        return value switch {
            null => false,
            bool b => b,
            string s => !string.IsNullOrEmpty(s) && !s.Equals("false", StringComparison.OrdinalIgnoreCase),
            int i => i != 0,
            long l => l != 0,
            double d => d != 0.0 && !double.IsNaN(d),
            decimal dec => dec != 0,
            float f => f != 0.0f && !float.IsNaN(f),
            _ => true // Non-null objects are considered true
        };
    }

    /// <summary>
    /// Gets a string representation of this if PI showing the condition and result.
    /// </summary>
    /// <returns>A string in the format "if(condition_type: 'condition_value') = result".</returns>
    public override string ToString() {
        var conditionType = ConditionExpression?.GetType().Name ?? "null";
        var conditionValue = ConditionExpression?.ToString() ?? "null";
        return $"if({conditionType}: '{conditionValue}') = {ConditionMet}";
    }
}
