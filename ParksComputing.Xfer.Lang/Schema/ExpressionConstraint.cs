using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Schema;

/// <summary>
/// Represents a constraint that uses logical expressions to validate fields in a document.
/// Supports operators like "any" and "all" to check field presence or other conditions.
/// </summary>
public class ExpressionConstraint {
    /// <summary>
    /// Gets or sets the logical operator for the expression ("any", "all", etc.).
    /// </summary>
    public string Operator { get; set; } = string.Empty; // "any", "all"

    /// <summary>
    /// Gets or sets the list of field names that the expression operates on.
    /// </summary>
    public List<string> Fields { get; set; } = [];

    /// <summary>
    /// Evaluates the expression constraint against the specified document.
    /// </summary>
    /// <param name="document">The object element to evaluate against.</param>
    /// <returns>True if the expression constraint is satisfied; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an unsupported operator is used.</exception>
    public bool Evaluate(ObjectElement document) {
        return Operator switch {
            "any" => Fields.Any(field => document.Dictionary.ContainsKey(field)),
            "all" => Fields.All(field => document.Dictionary.ContainsKey(field)),
            _ => throw new InvalidOperationException($"Unsupported operator: {Operator}")
        };
    }
}
