using System;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Scripting.Comparison;

public class LessThanOperator : ScriptingOperator {
    public override string OperatorName => "lt";
    public override string Description => "Returns true if left < right";
    public override int MinArguments => 2; public override int MaxArguments => 2;
    public override object? Evaluate(ScriptingContext context, params Element[] arguments) {
        ValidateArguments(arguments);
        var left = ResolveValue(arguments[0], context);
        var right = ResolveValue(arguments[1], context);
        if (left == null || right == null) {
            return false; // null is never < anything (including null)
        }

        return Compare(left, right) < 0;
    }
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
