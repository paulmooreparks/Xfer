using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Schema;

public class SchemaField {
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Data type: string, integer, etc.
    public bool IsRequired { get; set; } = false;
    public Func<ListElement, bool>? CustomValidation { get; set; } = null;
}
