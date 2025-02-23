using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Schema;

public class ExpressionConstraint {
    public string Operator { get; set; } = string.Empty; // "any", "all"
    public List<string> Fields { get; set; } = new();

    public bool Evaluate(ObjectElement document) {
        return Operator switch {
            "any" => Fields.Any(field => document.Values.ContainsKey(field)),
            "all" => Fields.All(field => document.Values.ContainsKey(field)),
            _ => throw new InvalidOperationException($"Unsupported operator: {Operator}")
        };
    }
}
