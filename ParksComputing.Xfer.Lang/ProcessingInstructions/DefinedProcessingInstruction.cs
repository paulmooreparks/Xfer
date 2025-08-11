using System;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;
using ParksComputing.Xfer.Lang.Scripting;
using ParksComputing.Xfer.Lang.Scripting.Utility;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions;

/// <summary>
/// Processing instruction for checking if any element is defined (has a meaningful value).
/// Returns true if the element exists and has a non-null, non-empty value, false otherwise.
/// This works like #ifdef but for any element type - dynamic elements, characters, references, etc.
/// Integrates with the dynamic element system for powerful conditional logic.
/// </summary>
/// <example>
/// Environment variable checking:
/// <code>
/// &lt;! defined &lt;|DEBUG_MODE|&gt; !&gt;
/// &lt;! defined &lt;|PATH|&gt; !&gt;
/// </code>
///
/// Character elements (always defined):
/// <code>
/// &lt;! defined #42 !&gt;
/// </code>
///
/// Reference elements (when implemented):
/// <code>
/// &lt;! defined &lt;`some-reference`&gt; !&gt;
/// </code>
///
/// With dynamic source configuration:
/// <code>
/// &lt;! dynamicSource { debug env &quot;DEBUG_MODE&quot; } !&gt;
/// &lt;! defined &lt;|debug|&gt; !&gt;
/// </code>
/// </example>
public class DefinedProcessingInstruction : ProcessingInstruction {
    /// <summary>
    /// The keyword identifier for this processing instruction.
    /// </summary>
    public const string Keyword = "defined";

    /// <summary>
    /// The element to check for definition (any element type).
    /// </summary>
    public Element SourceElement { get; private set; }

    /// <summary>
    /// The result of the definition check - true if the element is defined (has meaningful value), false otherwise.
    /// </summary>
    public bool IsDefined { get; private set; }

    /// <summary>
    /// The scripting engine used for operator evaluation (cached for performance).
    /// </summary>
    private static ScriptingEngine? _scriptingEngine;

    /// <summary>
    /// Gets or creates the scripting engine for evaluating the defined operator.
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
    /// Initializes a new instance of the DefinedProcessingInstruction class.
    /// </summary>
    /// <param name="value">The element to check for definition (any element type).</param>
    public DefinedProcessingInstruction(Element value) : base(value, Keyword) {
        SourceElement = value;
    }

    /// <summary>
    /// Handles the processing of the element definition check.
    /// This performs the actual conditional evaluation and stores the result.
    /// Now delegates to the DefinedOperator in the Scripting namespace for consistent logic.
    /// </summary>
    public override void ProcessingInstructionHandler() {
        try {
            // Use the new DefinedOperator for evaluation
            var scriptingEngine = GetScriptingEngine();
            var result = scriptingEngine.Evaluate("defined", SourceElement);

            // The DefinedOperator returns a boolean result
            IsDefined = result is bool boolResult && boolResult;
        }
        catch (Exception) {
            // If any error occurs during evaluation, consider it not defined
            IsDefined = false;
        }
    }

    /// <summary>
    /// Handles element processing for the defined PI.
    /// This is typically not used for conditional PIs as they evaluate during processing instruction handling.
    /// </summary>
    /// <param name="element">The element to process (unused for this PI type).</param>
    public override void ElementHandler(Element element) {
        // Conditional PIs typically don't need element-specific processing
        // The evaluation happens during ProcessingInstructionHandler()
    }

    /// <summary>
    /// Gets a string representation of this defined PI showing the element and result.
    /// </summary>
    /// <returns>A string in the format "defined(element_type: 'element_value') = result".</returns>
    public override string ToString() {
        var elementType = SourceElement?.GetType().Name ?? "null";
        var elementValue = SourceElement?.ToString() ?? "null";
        return $"defined({elementType}: '{elementValue}') = {IsDefined}";
    }
}
