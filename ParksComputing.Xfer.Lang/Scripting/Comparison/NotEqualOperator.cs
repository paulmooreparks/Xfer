using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Scripting.Comparison;

/// <summary>
/// Logical negation of EqualsOperator (eq). Returns true when operands are not equal.
/// </summary>
public class NotEqualOperator : ScriptingOperator {
    public override string OperatorName => "ne";
    public override string Description => "Returns true if left != right";
    public override int MinArguments => 2;
    public override int MaxArguments => 2;

    public override object? Evaluate(ScriptingContext context, params Element[] arguments) {
        ValidateArguments(arguments);
        // Reuse eq operator logic via EqualsOperator implementation
        var eq = new EqualsOperator();
        var equal = (bool)eq.Evaluate(context, arguments)!;
        return !equal;
    }
}
