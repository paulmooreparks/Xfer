using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Schema;

public class SchemaObject {
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, SchemaField> Fields { get; set; } = [];
}
