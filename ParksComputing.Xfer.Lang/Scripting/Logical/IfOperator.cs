using System;
using System.Collections.Generic;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Scripting.Logical;

/// <summary>
/// Scripting operator providing conditional evaluation. Returns one of two values depending on the truthiness
/// of the first argument. Usage: <c>if condition trueValue [falseValue]</c>. Also leveraged by the <c>if</c>
/// processing instruction. Nested operator expressions (for example a defined test like: <c>if defined DEREF</c>) are supported.
/// NOTE: Angle-bracket dereference placeholders were removed from the XML documentation example to avoid malformed XML warnings.
/// </summary>
public class IfOperator : ScriptingOperator {
    /// <summary>
    /// Gets the unique name identifier for this operator.
    /// </summary>
    public override string OperatorName => "if";

    /// <summary>
    /// Gets a brief description of what this operator does.
    /// </summary>
    public override string Description => "Evaluates a condition and returns one of two values based on the result";

    /// <summary>
    /// Gets the minimum number of arguments this operator requires.
    /// The if operator requires at least 2 arguments: condition and true-value.
    /// </summary>
    public override int MinArguments => 2;

    /// <summary>
    /// Gets the maximum number of arguments this operator accepts.
    /// The if operator accepts at most 3 arguments: condition, true-value, and false-value.
    /// </summary>
    public override int MaxArguments => 3;

    /// <summary>
    /// Evaluates a conditional expression and returns the appropriate value.
    ///
    /// Conditional semantics:
    /// - First argument: The condition to evaluate (any element or expression)
    /// - Second argument: Value to return if condition is true
    /// - Third argument (optional): Value to return if condition is false (defaults to null)
    ///
    /// Truthiness evaluation:
    /// - Boolean true/false values are used directly
    /// - Non-null, non-empty strings are considered true
    /// - Non-zero numbers are considered true
    /// - Non-null objects are considered true
    /// - null, empty strings, and zero are considered false
    /// </summary>
    /// <param name="context">The scripting context containing variables and environment information.</param>
    /// <param name="arguments">The condition and value arguments. Must contain 2-3 elements.</param>
    /// <returns>The true-value if condition is true, false-value if condition is false, or null if no false-value provided.</returns>
    public override object? Evaluate(ScriptingContext context, params Element[] arguments) {
        ValidateArguments(arguments);

        var conditionElement = arguments[0];
        var trueValueElement = arguments[1];
        Element? falseValueElement = arguments.Length > 2 ? arguments[2] : null;

        // Evaluate the condition
        bool conditionResult = EvaluateCondition(conditionElement, context);

        // Return appropriate value based on condition
        if (conditionResult) {
            return ResolveValue(trueValueElement, context);
        } else {
            return falseValueElement != null ? ResolveValue(falseValueElement, context) : null;
        }
    }

    /// <summary>
    /// Evaluates the condition element to determine its truthiness.
    /// This method handles various element types and applies consistent truthiness rules.
    /// </summary>
    /// <param name="conditionElement">The element representing the condition to evaluate.</param>
    /// <param name="context">The scripting context for variable resolution.</param>
    /// <returns>True if the condition is considered true; otherwise, false.</returns>
    private bool EvaluateCondition(Element conditionElement, ScriptingContext context) {
        // Handle direct operator expressions (nested operator calls)
        if (IsOperatorExpression(conditionElement)) {
            return EvaluateOperatorExpression(conditionElement, context);
        }

        // For regular elements, resolve the value and apply truthiness rules
        var conditionValue = ResolveValue(conditionElement, context);
        return ConvertToBoolean(conditionValue);
    }

    /// <summary>
    /// Determines if an element represents an operator expression that should be evaluated.
    /// This allows for nested operator calls like: <c>if defined _foo</c> (example simplified for XML docs).
    /// </summary>
    /// <param name="element">The element to check.</param>
    /// <returns>True if the element represents an operator expression; otherwise, false.</returns>
    private bool IsOperatorExpression(Element element) {
        // For now, we'll identify operator expressions by checking if the element
        // contains what looks like an operator name followed by arguments
        // This is a simplified approach - a full implementation might use a more
        // sophisticated expression parser

        if (element is CollectionElement collection && collection.Count >= 2) {
            // Check if first element looks like an operator name
            var firstElement = collection.GetElementAt(0);
            if (firstElement is TextElement textElement) {
                var operatorName = textElement.Value?.Trim();
                return !string.IsNullOrEmpty(operatorName) &&
                       IsKnownOperator(operatorName);
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a string represents a known operator name by querying the global operator registry.
    /// </summary>
    /// <param name="operatorName">The potential operator name to check.</param>
    /// <returns>True if the name represents a known operator; otherwise, false.</returns>
    private bool IsKnownOperator(string operatorName) {
        // Query the global operator registry for registered operators
        return OperatorRegistry.IsOperatorRegistered(operatorName);
    }

    /// <summary>
    /// Evaluates an operator expression by extracting the operator name and arguments,
    /// then delegating to the appropriate operator.
    /// </summary>
    /// <param name="element">The collection element containing the operator expression.</param>
    /// <param name="context">The scripting context for evaluation.</param>
    /// <returns>The result of the operator evaluation, converted to boolean.</returns>
    private bool EvaluateOperatorExpression(Element element, ScriptingContext context) {
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

            // Create a temporary scripting engine to evaluate the nested operator
            // In a full implementation, this would reuse the existing engine
            var nestedEngine = new ScriptingEngine(context);
            var result = nestedEngine.Evaluate(operatorName, operatorArguments);

            // Convert result to boolean
            return ConvertToBoolean(result);
        }
        catch {
            // If operator evaluation fails, consider it false
            return false;
        }
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
    /// Provides additional validation specific to the if operator.
    /// </summary>
    /// <param name="arguments">The arguments to validate.</param>
    /// <exception cref="ArgumentException">Thrown when arguments don't meet the if operator's requirements.</exception>
    protected override void ValidateArguments(Element[] arguments) {
        base.ValidateArguments(arguments);

        // Additional validation: ensure we have a condition
        if (arguments[0] == null) {
            throw new ArgumentException("The 'if' operator requires a non-null condition element");
        }

        // True-value can be null (it's a valid return value)
        // False-value can be null or missing (optional argument)
    }

    /// <summary>
    /// Returns a detailed string representation of this operator.
    /// </summary>
    /// <returns>A string describing the operator and its usage.</returns>
    public override string ToString() {
        return $"{OperatorName}: {Description} - Usage: if(condition, trueValue[, falseValue])";
    }
}
