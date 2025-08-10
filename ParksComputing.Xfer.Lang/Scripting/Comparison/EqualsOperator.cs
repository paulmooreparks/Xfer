using System;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Scripting.Comparison;

/// <summary>
/// Scripting operator that checks if two elements are equal.
/// Provides flexible equality comparison handling different element types and values.
/// </summary>
public class EqualsOperator : ScriptingOperator {
    /// <summary>
    /// Gets the unique name identifier for this operator.
    /// </summary>
    public override string OperatorName => "eq";

    /// <summary>
    /// Gets a brief description of what this operator does.
    /// </summary>
    public override string Description => "Checks if two elements are equal";

    /// <summary>
    /// Gets the minimum number of arguments this operator requires.
    /// The equals operator requires exactly two arguments to compare.
    /// </summary>
    public override int MinArguments => 2;

    /// <summary>
    /// Gets the maximum number of arguments this operator accepts.
    /// The equals operator accepts exactly two arguments.
    /// </summary>
    public override int MaxArguments => 2;

    /// <summary>
    /// Evaluates whether two elements are equal.
    ///
    /// Equality semantics:
    /// - Resolves both elements to their actual values using context
    /// - Handles null comparisons (null == null is true, null != anything else)
    /// - Performs type-appropriate comparisons:
    ///   - String comparisons are case-sensitive by default
    ///   - Numeric comparisons handle type coercion (int vs double, etc.)
    ///   - Boolean comparisons are direct
    ///   - Object comparisons use .Equals() method
    /// </summary>
    /// <param name="context">The scripting context containing variables and environment information.</param>
    /// <param name="arguments">The two elements to compare. Must contain exactly two elements.</param>
    /// <returns>Boolean true if the elements are equal; otherwise, false.</returns>
    public override object? Evaluate(ScriptingContext context, params Element[] arguments) {
        ValidateArguments(arguments);

        var leftElement = arguments[0];
        var rightElement = arguments[1];

        // Resolve both elements to their actual values
        var leftValue = ResolveValue(leftElement, context);
        var rightValue = ResolveValue(rightElement, context);

        // Handle null comparisons
        if (leftValue == null && rightValue == null) {
            return true;
        }

        if (leftValue == null || rightValue == null) {
            return false;
        }

        // Perform type-appropriate equality comparison
        return PerformEqualityComparison(leftValue, rightValue);
    }

    /// <summary>
    /// Performs the actual equality comparison between two resolved values.
    /// </summary>
    /// <param name="leftValue">The left value to compare.</param>
    /// <param name="rightValue">The right value to compare.</param>
    /// <returns>True if the values are equal; otherwise, false.</returns>
    private bool PerformEqualityComparison(object leftValue, object rightValue) {
        var leftType = leftValue.GetType();
        var rightType = rightValue.GetType();

        // Same type - use direct comparison
        if (leftType == rightType) {
            return leftValue.Equals(rightValue);
        }

        // Handle numeric type coercion
        if (IsNumeric(leftType) && IsNumeric(rightType)) {
            return CompareNumericValues(leftValue, rightValue);
        }

        // Handle string conversions - convert both to strings and compare
        if (leftType == typeof(string) || rightType == typeof(string)) {
            return leftValue.ToString() == rightValue.ToString();
        }

        // Handle boolean conversions
        if (leftType == typeof(bool) || rightType == typeof(bool)) {
            return CompareBooleanValues(leftValue, rightValue);
        }

        // Fall back to string comparison for complex types
        return leftValue.ToString() == rightValue.ToString();
    }

    /// <summary>
    /// Compares two numeric values with appropriate type coercion.
    /// </summary>
    /// <param name="leftValue">The left numeric value.</param>
    /// <param name="rightValue">The right numeric value.</param>
    /// <returns>True if the numeric values are equal; otherwise, false.</returns>
    private bool CompareNumericValues(object leftValue, object rightValue) {
        try {
            // Convert both to decimal for high-precision comparison
            var leftDecimal = Convert.ToDecimal(leftValue);
            var rightDecimal = Convert.ToDecimal(rightValue);
            return leftDecimal == rightDecimal;
        }
        catch (OverflowException) {
            // If decimal conversion fails due to overflow, fall back to double comparison
            try {
                var leftDouble = Convert.ToDouble(leftValue);
                var rightDouble = Convert.ToDouble(rightValue);
                return Math.Abs(leftDouble - rightDouble) < double.Epsilon;
            }
            catch {
                // If all numeric conversions fail, use string comparison as last resort
                return leftValue.ToString() == rightValue.ToString();
            }
        }
    }

    /// <summary>
    /// Compares values where at least one is boolean.
    /// </summary>
    /// <param name="leftValue">The left value.</param>
    /// <param name="rightValue">The right value.</param>
    /// <returns>True if the values are equal when treated as booleans; otherwise, false.</returns>
    private bool CompareBooleanValues(object leftValue, object rightValue) {
        try {
            var leftBool = ConvertToBoolean(leftValue);
            var rightBool = ConvertToBoolean(rightValue);
            return leftBool == rightBool;
        }
        catch {
            // If boolean conversion fails, fall back to string comparison
            return leftValue.ToString() == rightValue.ToString();
        }
    }

    /// <summary>
    /// Converts a value to boolean using common truthiness rules.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The boolean representation of the value.</returns>
    private bool ConvertToBoolean(object value) {
        return value switch {
            bool b => b,
            string s => !string.IsNullOrEmpty(s) && !s.Equals("false", StringComparison.OrdinalIgnoreCase),
            int i => i != 0,
            long l => l != 0,
            double d => d != 0.0,
            decimal dec => dec != 0,
            _ => true // Non-null objects are considered true
        };
    }

    /// <summary>
    /// Provides additional validation specific to the equals operator.
    /// </summary>
    /// <param name="arguments">The arguments to validate.</param>
    protected override void ValidateArguments(Element[] arguments) {
        base.ValidateArguments(arguments);

        // Both arguments can be null (null == null is a valid comparison)
        // No additional validation needed for equals operator
    }

    /// <summary>
    /// Returns a detailed string representation of this operator.
    /// </summary>
    /// <returns>A string describing the operator and its usage.</returns>
    public override string ToString() {
        return $"{OperatorName}: {Description} - Usage: eq(element1, element2)";
    }
}
