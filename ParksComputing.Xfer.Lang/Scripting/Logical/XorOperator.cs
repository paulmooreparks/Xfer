using System;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Scripting.Logical;

public class XorOperator : ScriptingOperator {
    public override string OperatorName => "xor";
    public override string Description => "Logical exclusive OR";
    public override int MinArguments => 2; public override int MaxArguments => 2;
    public override object? Evaluate(ScriptingContext context, params Element[] arguments) {
        ValidateArguments(arguments);
        var a = ToBoolean(ResolveValue(arguments[0], context));
        var b = ToBoolean(ResolveValue(arguments[1], context));
        return a ^ b;
    }
    private bool ToBoolean(object? v) => v switch {
        null => false,
        bool b => b,
        string s => !string.IsNullOrEmpty(s) && !s.Equals("false", StringComparison.OrdinalIgnoreCase),
        int i => i != 0,
        long l => l != 0,
        double d => d != 0 && !double.IsNaN(d),
        decimal m => m != 0,
        float f => f != 0 && !float.IsNaN(f),
        _ => true
    };
}
