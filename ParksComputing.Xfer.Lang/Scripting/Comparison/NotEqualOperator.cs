using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Scripting.Comparison;

/// <summary>
/// Logical negation of EqualsOperator (eq). Returns true when operands are not equal.
/// </summary>
public class NotEqualOperator : ScriptingOperator {
    /// <inheritdoc />
    public override string OperatorName => "ne";

    /// <inheritdoc />
    public override string Description => "Returns true if left != right";

    /// <inheritdoc />
    public override int MinArguments => 2;

    /// <inheritdoc />
    public override int MaxArguments => 2;

    /// <summary>
    /// Evaluates the operator returning <c>true</c> when the two resolved argument values are not equal.
    /// Delegates equality logic to <see cref="EqualsOperator"/> and negates the result.
    /// </summary>
    /// <param name="context">Active scripting context used to resolve element values.</param>
    /// <param name="arguments">Exactly two elements to compare.</param>
    /// <returns><c>true</c> if arguments are not equal; otherwise <c>false</c>.</returns>
    public override object? Evaluate(ScriptingContext context, params Element[] arguments) {
        ValidateArguments(arguments);
        // Reuse eq operator logic via EqualsOperator implementation
        var eq = new EqualsOperator();
        var equal = (bool)eq.Evaluate(context, arguments)!;
        return !equal;
    }
}
