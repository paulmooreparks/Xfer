using System;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Scripting.Logical;

/// <summary>
/// Logical disjunction operator that evaluates arguments left-to-right and short-circuits on the first truthy value.
/// </summary>
public class OrOperator : ScriptingOperator {
    /// <inheritdoc />
    public override string OperatorName => "or";
    /// <inheritdoc />
    public override string Description => "Logical OR with short-circuit";
    /// <inheritdoc />
    public override int MinArguments => 2;
    /// <summary>Unlimited additional arguments may be supplied.</summary>
    public override int MaxArguments => int.MaxValue;
    /// <summary>Returns <c>true</c> for the first truthy argument; otherwise <c>false</c>.</summary>
    public override object? Evaluate(ScriptingContext context, params Element[] arguments) {
        ValidateArguments(arguments);
        foreach (var arg in arguments) {
            var val = ResolveValue(arg, context);
            if (ToBoolean(val)) {
                return true;
            }
        }
        return false;
    }
    /// <summary>Applies standard truthiness conversion rules.</summary>
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
