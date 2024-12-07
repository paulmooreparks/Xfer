using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Schema;

public class Constraint {
    public string Name { get; set; } = string.Empty; // e.g., "required"
    public object? Value { get; set; } // Boolean or evaluable expression
}
