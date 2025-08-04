using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Schema;

/// <summary>
/// Represents a field definition within a schema object or structure.
/// Defines the field's name, type, requirement status, and optional custom validation logic.
/// </summary>
public class SchemaField {
    /// <summary>
    /// Gets or sets the name of the schema field.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data type of the field (e.g., "string", "integer", "boolean").
    /// </summary>
    public string Type { get; set; } = string.Empty; // Data type: string, integer, etc.

    /// <summary>
    /// Gets or sets a value indicating whether this field is required in the schema.
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Gets or sets an optional custom validation function for complex field validation scenarios.
    /// The function receives a ListElement and returns true if validation passes.
    /// </summary>
    public Func<ListElement, bool>? CustomValidation { get; set; } = null;
}
