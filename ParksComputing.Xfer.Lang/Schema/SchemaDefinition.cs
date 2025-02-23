using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Schema;

public class SchemaDefinition {
    public string Name { get; set; } = string.Empty; // e.g., "address"
    public string Type { get; set; } = string.Empty; // e.g., "object", "element"
    public Dictionary<string, SchemaField>? Fields { get; set; } // For objects
    public string? ElementType { get; set; } // For array or element
    public List<Constraint>? Constraints { get; set; } // Validation constraints
}