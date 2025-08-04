using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Schema;

/// <summary>
/// Defines the structure and validation rules for a schema type in XferLang.
/// Can represent objects, arrays, or individual elements with associated constraints and field definitions.
/// </summary>
public class SchemaDefinition {
    /// <summary>
    /// Gets or sets the name of the schema definition (e.g., "address", "person").
    /// </summary>
    public string Name { get; set; } = string.Empty; // e.g., "address"

    /// <summary>
    /// Gets or sets the type category of the schema ("object", "array", "element", etc.).
    /// </summary>
    public string Type { get; set; } = string.Empty; // e.g., "object", "element"

    /// <summary>
    /// Gets or sets the field definitions for object-type schemas.
    /// Maps field names to their corresponding schema field specifications.
    /// </summary>
    public Dictionary<string, SchemaField>? Fields { get; set; } // For objects

    /// <summary>
    /// Gets or sets the element type for array or collection schemas.
    /// Specifies what type of elements the array contains.
    /// </summary>
    public string? ElementType { get; set; } // For array or element

    /// <summary>
    /// Gets or sets the validation constraints that apply to this schema definition.
    /// </summary>
    public List<Constraint>? Constraints { get; set; } // Validation constraints
}
