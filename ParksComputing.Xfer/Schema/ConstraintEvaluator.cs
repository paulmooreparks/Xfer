using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Elements;


namespace ParksComputing.Xfer.Schema;

public class ConstraintEvaluator {
    public static bool Evaluate(object? value, ObjectElement document) {
        // Handle boolean literals directly
        if (value is bool boolValue)
            return boolValue;

        // Evaluate expressions like "any", "all", or custom conditions
        if (value is ExpressionConstraint expression) {
            return expression.Evaluate(document);
        }

        throw new InvalidOperationException($"Unsupported constraint value: {value}");
    }
}
