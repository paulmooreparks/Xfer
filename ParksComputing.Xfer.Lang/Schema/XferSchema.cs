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
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Dictionary<string, SchemaDefinition> Definitions { get; set; } = [];
}
