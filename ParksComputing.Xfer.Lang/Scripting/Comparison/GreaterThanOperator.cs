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

#if DEBUG
        try {
            // Targeted trace for intermittent gt[#1 #2] anomaly observed in tests
            if (left is not null && right is not null) {
                string tag = left.ToString() == "1" && right.ToString() == "2" ? "[TRACE-GT-ANOMALY-CANDIDATE]" : "[TRACE-GT]";
                Console.WriteLine($"{tag} GreaterThanOperator.Evaluate left={left}({left.GetType().Name}) right={right}({right.GetType().Name}) numericLeft={IsNumericType(left.GetType())} numericRight={IsNumericType(right.GetType())}");
            }
        } catch { /* best effort */ }
#endif

    if (left == null || right == null) {
#if DEBUG
        Console.WriteLine("[TRACE-GT] Null operand detected -> result false");
#endif
        return false; // null not greater than anything
    }

        // Numeric path
    if (IsNumericType(left.GetType()) && IsNumericType(right.GetType())) {
            try {
                var ld = Convert.ToDecimal(left);
                var rd = Convert.ToDecimal(right);
#if DEBUG
                Console.WriteLine($"[TRACE-GT] Numeric compare {ld} > {rd} => {ld > rd}");
#endif
                return ld > rd;
            }
            catch (Exception) { /* fall through to string */ }
        }

        // DateTime comparisons
    if (left is DateTime ldt && right is DateTime rdt) {
#if DEBUG
        Console.WriteLine($"[TRACE-GT] DateTime compare {ldt:o} > {rdt:o} => {ldt > rdt}");
#endif
        return ldt > rdt;
        }

        // String / fallback: compare ordinal string representations
        var ls = left.ToString();
        var rs = right.ToString();
    var cmp = string.Compare(ls, rs, StringComparison.Ordinal) > 0;
#if DEBUG
    Console.WriteLine($"[TRACE-GT] Fallback string compare '{ls}' > '{rs}' => {cmp}");
#endif
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
