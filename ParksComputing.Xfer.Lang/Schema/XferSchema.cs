using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Schema;

/// <summary>
/// Represents an XferLang schema that defines the structure, validation rules,
/// and constraints for XferLang documents. Contains schema definitions,
/// type specifications, and validation metadata.
/// </summary>
public class XferSchema {
    /// <summary>
    /// Gets or sets the name of the schema.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional description of the schema's purpose and usage.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the collection of schema definitions contained in this schema.
    /// Maps definition names to their corresponding SchemaDefinition objects.
    /// </summary>
    public Dictionary<string, SchemaDefinition> Definitions { get; set; } = [];
}
