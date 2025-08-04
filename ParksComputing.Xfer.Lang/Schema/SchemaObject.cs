using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Schema;

/// <summary>
/// Represents a structured object definition within a schema.
/// Contains a collection of named fields that define the object's structure and validation rules.
/// </summary>
public class SchemaObject {
    /// <summary>
    /// Gets or sets the name of the schema object.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of fields that make up this schema object.
    /// Maps field names to their corresponding SchemaField definitions.
    /// </summary>
    public Dictionary<string, SchemaField> Fields { get; set; } = [];
}
