using System;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Scripting.Logical;

public class AndOperator : ScriptingOperator {
    public override string OperatorName => "and";
    public override string Description => "Logical AND with short-circuit";
    public override int MinArguments => 2; public override int MaxArguments => int.MaxValue;
    public override object? Evaluate(ScriptingContext context, params Element[] arguments) {
        ValidateArguments(arguments);
        foreach (var arg in arguments) {
            var val = ResolveValue(arg, context);
            if (!ToBoolean(val)) return false;
        }
        return true;
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
