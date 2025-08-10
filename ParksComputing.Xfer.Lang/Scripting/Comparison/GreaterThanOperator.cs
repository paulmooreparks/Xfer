using System;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Scripting.Comparison;

/// <summary>
/// Scripting operator that determines whether the first argument is greater than the second.
/// Supports numeric coercion; for non-numeric comparable types (string) falls back to ordinal comparison.
/// </summary>
public class GreaterThanOperator : ScriptingOperator {
    public override string OperatorName => "gt";
    public override string Description => "Returns true if left > right";
    public override int MinArguments => 2;
    public override int MaxArguments => 2;

    public override object? Evaluate(ScriptingContext context, params Element[] arguments) {
        ValidateArguments(arguments);

        var left = ResolveValue(arguments[0], context);
        var right = ResolveValue(arguments[1], context);

        if (left == null || right == null) return false; // null not greater than anything

        // Numeric path
    if (IsNumericType(left.GetType()) && IsNumericType(right.GetType())) {
            try {
                var ld = Convert.ToDecimal(left);
                var rd = Convert.ToDecimal(right);
                return ld > rd;
            }
            catch (Exception) { /* fall through to string */ }
        }

        // DateTime comparisons
        if (left is DateTime ldt && right is DateTime rdt) {
            return ldt > rdt;
        }

        // String / fallback: compare ordinal string representations
        var ls = left.ToString();
        var rs = right.ToString();
        return string.Compare(ls, rs, StringComparison.Ordinal) > 0;
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
