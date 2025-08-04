using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang.Elements;


namespace ParksComputing.Xfer.Lang.Schema;

/// <summary>
/// Provides static methods for evaluating schema constraints against document values.
/// Handles boolean literals, expression constraints, and custom validation logic.
/// </summary>
public class ConstraintEvaluator {
    /// <summary>
    /// Evaluates a constraint value against a document context.
    /// Supports boolean literals, expression constraints, and complex validation scenarios.
    /// </summary>
    /// <param name="value">The constraint value to evaluate (boolean, expression, etc.).</param>
    /// <param name="document">The document context for expression evaluation.</param>
    /// <returns>True if the constraint is satisfied; otherwise, false.</returns>
    public static bool Evaluate(object? value, ObjectElement document) {
        // Handle boolean literals directly
        if (value is bool boolValue) {
            return boolValue;
        }

        // Evaluate expressions like "any", "all", or custom conditions
        if (value is ExpressionConstraint expression) {
            return expression.Evaluate(document);
        }

        throw new InvalidOperationException($"Unsupported constraint value: {value}");
    }
}
