using System;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Scripting.Comparison;

/// <summary>
/// Scripting operator that determines whether the first (left) argument is less than or equal to the second (right) argument.
/// Null handling: <c>null &lt;= null</c> returns true; any other combination with <c>null</c> returns false.
/// </summary>
public class LessThanOrEqualOperator : ScriptingOperator {
    /// <inheritdoc />
    public override string OperatorName => "lte";

    /// <inheritdoc />
    public override string Description => "Returns true if left &lt;= right";

    /// <inheritdoc />
    public override int MinArguments => 2;

    /// <inheritdoc />
    public override int MaxArguments => 2;

    /// <summary>
    /// Evaluates the operator returning <c>true</c> when left &lt;= right after resolving argument values.
    /// </summary>
    /// <param name="context">Active scripting context used to resolve element values.</param>
    /// <param name="arguments">Exactly two elements: left and right.</param>
    /// <returns><c>true</c> if left &lt;= right; otherwise <c>false</c>. When both operands are <c>null</c> returns <c>true</c>.</returns>
    public override object? Evaluate(ScriptingContext context, params Element[] arguments) {
        ValidateArguments(arguments);
        var left = ResolveValue(arguments[0], context);
        var right = ResolveValue(arguments[1], context);
        if (left == null || right == null) {
            return left == null && right == null; // null <= null => true
        }

        return Compare(left, right) <= 0;
    }

    /// <summary>
    /// Performs a type‑aware comparison returning a negative/zero/positive value.
    /// </summary>
    private int Compare(object left, object right) {
        if (IsNumeric(left.GetType()) && IsNumeric(right.GetType())) {
            try { return Decimal.Compare(Convert.ToDecimal(left), Convert.ToDecimal(right)); } catch { }
            return Math.Sign(Convert.ToDouble(left) - Convert.ToDouble(right));
        }
        if (left is DateTime ldt && right is DateTime rdt) {
            return ldt.CompareTo(rdt);
        }

        return string.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal);
    }
}
