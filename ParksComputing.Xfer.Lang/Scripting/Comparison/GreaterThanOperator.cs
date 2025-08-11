using System;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Scripting.Comparison;

/// <summary>
/// Scripting operator that determines whether the first argument is greater than the second.
/// Supports numeric coercion; for non-numeric comparable types (string) falls back to ordinal comparison.
/// </summary>
public class GreaterThanOperator : ScriptingOperator {
    /// <inheritdoc />
    public override string OperatorName => "gt";

    /// <inheritdoc />
    public override string Description => "Returns true if left > right";

    /// <inheritdoc />
    public override int MinArguments => 2;

    /// <inheritdoc />
    public override int MaxArguments => 2;

    /// <summary>
    /// Evaluates the operator returning <c>true</c> when the resolved left value is strictly greater than the resolved right value.
    /// </summary>
    /// <param name="context">Active scripting context used to resolve element values.</param>
    /// <param name="arguments">Exactly two elements: left and right.</param>
    /// <returns><c>true</c> if left > right; otherwise <c>false</c>. If either operand is <c>null</c> the result is <c>false</c>.</returns>
    public override object? Evaluate(ScriptingContext context, params Element[] arguments) {
        ValidateArguments(arguments);

        var left = ResolveValue(arguments[0], context);
        var right = ResolveValue(arguments[1], context);

    // Debug trace removed to reduce noise; can be reintroduced under DEBUG if needed for anomaly analysis.

    if (left == null || right == null) {
    // (debug trace suppressed)
        return false; // null not greater than anything
    }

        // Numeric path
    if (IsNumericType(left.GetType()) && IsNumericType(right.GetType())) {
            try {
                var ld = Convert.ToDecimal(left);
                var rd = Convert.ToDecimal(right);
                // (debug trace suppressed)
                return ld > rd;
            }
            catch (Exception) { /* fall through to string */ }
        }

        // DateTime comparisons
    if (left is DateTime ldt && right is DateTime rdt) {
    // (debug trace suppressed)
        return ldt > rdt;
        }

        // String / fallback: compare ordinal string representations
        var ls = left.ToString();
        var rs = right.ToString();
    var cmp = string.Compare(ls, rs, StringComparison.Ordinal) > 0;
    // (debug trace suppressed)
    return cmp;
    }

    private static bool IsNumericType(Type t) {
    if (t.IsEnum) { return false; }
        switch (Type.GetTypeCode(t)) {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
    }
}
