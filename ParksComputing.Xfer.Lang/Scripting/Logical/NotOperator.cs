using System;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Scripting.Logical;

/// <summary>
/// Logical negation operator that inverts the truthiness of a single argument.
/// </summary>
public class NotOperator : ScriptingOperator {
    /// <inheritdoc />
    public override string OperatorName => "not";
    /// <inheritdoc />
    public override string Description => "Logical negation";
    /// <inheritdoc />
    public override int MinArguments => 1;
    /// <summary>Exactly one argument.</summary>
    public override int MaxArguments => 1;
    /// <summary>Returns <c>true</c> when the argument is falsey; otherwise <c>false</c>.</summary>
    public override object? Evaluate(ScriptingContext context, params Element[] arguments) {
        ValidateArguments(arguments);
        var val = ResolveValue(arguments[0], context);
        return !ToBoolean(val);
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
