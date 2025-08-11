using System;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Scripting.Comparison;

/// <summary>
/// Scripting operator that determines whether the first (left) argument is strictly less than the second (right) argument.
/// Supports numeric coercion, <see cref="DateTime"/> comparison, and falls back to ordinal string comparison when types differ.
/// Symbol: <c>lt</c>
/// </summary>
public class LessThanOperator : ScriptingOperator {
    /// <inheritdoc />
    public override string OperatorName => "lt";

    /// <inheritdoc />
    public override string Description => "Returns true if left < right";

    /// <inheritdoc />
    public override int MinArguments => 2;

    /// <inheritdoc />
    public override int MaxArguments => 2;

    /// <summary>
    /// Evaluates the operator returning <c>true</c> when the resolved left value is strictly less than the resolved right value.
    /// </summary>
    /// <param name="context">Active scripting context used to resolve element values.</param>
    /// <param name="arguments">Exactly two elements: left and right.</param>
    /// <returns><c>true</c> if <paramref name="arguments"/>[0] is strictly less than <paramref name="arguments"/>[1]; otherwise <c>false</c>.
    /// Returns <c>false</c> if either side is <c>null</c>.</returns>
    public override object? Evaluate(ScriptingContext context, params Element[] arguments) {
        ValidateArguments(arguments);
        var left = ResolveValue(arguments[0], context);
        var right = ResolveValue(arguments[1], context);
        if (left == null || right == null) {
            return false; // null is never < anything (including null)
        }

        return Compare(left, right) < 0;
    }

    /// <summary>
    /// Performs a type‑aware comparison returning a negative value when <paramref name="left"/> is less than <paramref name="right"/>.
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
