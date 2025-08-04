using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Schema;

/// <summary>
/// Represents a validation constraint that can be applied to schema fields or objects.
/// Constraints define validation rules such as required fields, value ranges, patterns, or custom expressions.
/// </summary>
public class Constraint {
    /// <summary>
    /// Gets or sets the name of the constraint (e.g., "required", "minLength", "pattern").
    /// </summary>
    public string Name { get; set; } = string.Empty; // e.g., "required"

    /// <summary>
    /// Gets or sets the constraint value, which can be a boolean, string, number, or evaluable expression
    /// depending on the constraint type.
    /// </summary>
    public object? Value { get; set; } // Boolean or evaluable expression
}
