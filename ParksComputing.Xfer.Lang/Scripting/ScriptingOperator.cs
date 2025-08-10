using System;
using System.Collections.Generic;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Scripting;

/// <summary>
/// Abstract base class for scripting operators that evaluate expressions in XferLang.
/// Provides the foundation for comparison, logical, utility, and other expression operators
/// used in conditional logic and dynamic content resolution.
/// </summary>
public abstract class ScriptingOperator {
    /// <summary>
    /// Gets the unique name identifier for this operator.
    /// This name is used to register and lookup the operator in the scripting engine.
    /// </summary>
    public abstract string OperatorName { get; }

    /// <summary>
    /// Gets a brief description of what this operator does.
    /// Used for documentation and error messages.
    /// </summary>
    public virtual string Description => $"Scripting operator: {OperatorName}";

    /// <summary>
    /// Gets the minimum number of arguments this operator requires.
    /// </summary>
    public virtual int MinArguments => 1;

    /// <summary>
    /// Gets the maximum number of arguments this operator accepts.
    /// Return -1 for unlimited arguments.
    /// </summary>
    public virtual int MaxArguments => -1;

    /// <summary>
    /// Evaluates the operator with the specified arguments in the given context.
    /// </summary>
    /// <param name="context">The scripting context containing variables and environment information.</param>
    /// <param name="arguments">The arguments to evaluate. Number and types depend on the specific operator.</param>
    /// <returns>The result of the operator evaluation, or null if no result is available.</returns>
    public abstract object? Evaluate(ScriptingContext context, params Element[] arguments);

    /// <summary>
    /// Validates that the provided arguments meet the operator's requirements.
    /// </summary>
    /// <param name="arguments">The arguments to validate.</param>
    /// <exception cref="ArgumentException">Thrown when arguments don't meet requirements.</exception>
    protected virtual void ValidateArguments(Element[] arguments) {
        if (arguments == null) {
            throw new ArgumentNullException(nameof(arguments));
        }

        if (arguments.Length < MinArguments) {
            throw new ArgumentException($"Operator '{OperatorName}' requires at least {MinArguments} argument(s), got {arguments.Length}");
        }

        if (MaxArguments >= 0 && arguments.Length > MaxArguments) {
            throw new ArgumentException($"Operator '{OperatorName}' accepts at most {MaxArguments} argument(s), got {arguments.Length}");
        }
    }

    /// <summary>
    /// Resolves an element to its actual value, handling dynamic elements, typed elements, and collections.
    /// </summary>
    /// <param name="element">The element to resolve.</param>
    /// <param name="context">The scripting context for variable resolution.</param>
    /// <returns>The resolved value of the element.</returns>
    protected virtual object? ResolveValue(Element element, ScriptingContext context) {
        if (element == null) {
            return null;
        }

        // Handle dynamic elements with variable resolution
        if (element is DynamicElement dynamic) {
            return ResolveDynamicValue(dynamic, context);
        }

        // For all other elements, use their ParsedValue
        return element.ParsedValue;
    }

    /// <summary>
    /// Resolves a dynamic element's value using the scripting context.
    /// </summary>
    /// <param name="dynamic">The dynamic element to resolve.</param>
    /// <param name="context">The scripting context for variable resolution.</param>
    /// <returns>The resolved value, or null if the variable is not defined.</returns>
    private object? ResolveDynamicValue(DynamicElement dynamic, ScriptingContext context) {
        // Dynamic elements contain variable names - extract the variable name
        var variableName = dynamic.Value; // This contains the resolved variable name

        // Try to resolve from context variables
        if (context.TryResolveVariable(variableName, out var value)) {
            return value;
        }

        // If not found in context, use the dynamic element's own resolution
        // (This handles cases where the dynamic element was already resolved by the parser)
        return dynamic.ParsedValue;
    }

    /// <summary>
    /// Validates that two elements are comparable (same type or convertible types).
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <param name="context">The scripting context.</param>
    /// <exception cref="InvalidOperationException">Thrown when elements are not comparable.</exception>
    protected virtual void ValidateComparable(Element left, Element right, ScriptingContext context) {
        var leftValue = ResolveValue(left, context);
        var rightValue = ResolveValue(right, context);

        if (leftValue == null || rightValue == null) {
            return; // Null values are handled by individual operators
        }

        var leftType = leftValue.GetType();
        var rightType = rightValue.GetType();

        // Allow same types
        if (leftType == rightType) {
            return;
        }

        // Allow numeric conversions
        if (IsNumeric(leftType) && IsNumeric(rightType)) {
            return;
        }

        // Allow string conversions (everything can convert to string)
        if (leftType == typeof(string) || rightType == typeof(string)) {
            return;
        }

        throw new InvalidOperationException(
            $"Cannot compare {leftType.Name} and {rightType.Name} in operator '{OperatorName}'");
    }

    /// <summary>
    /// Determines if a type is numeric (can be used in numeric comparisons).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is numeric; otherwise, false.</returns>
    // Made public to allow lightweight operator implementations in other namespaces
    // (e.g., simple comparison operators) to reuse numeric detection without subclass
    // inheritance complexities or duplicate logic. Safe because it's pure and stateless.
    public static bool IsNumeric(Type type) {
        return type == typeof(int) || type == typeof(long) || type == typeof(double) ||
               type == typeof(decimal) || type == typeof(float) || type == typeof(short) ||
               type == typeof(byte) || type == typeof(uint) || type == typeof(ulong) ||
               type == typeof(ushort) || type == typeof(sbyte);
    }

    /// <summary>
    /// Returns a string representation of this operator.
    /// </summary>
    /// <returns>A string containing the operator name and description.</returns>
    public override string ToString() {
        return $"{OperatorName}: {Description}";
    }
}
