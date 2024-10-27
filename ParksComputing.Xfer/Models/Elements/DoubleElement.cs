using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class DoubleElement : Element {
    public double Value { get; set; } = 0.0;

    public const char OpeningMarker = '^';
    public const char ClosingMarker = OpeningMarker;

    public DoubleElement(double value) : base("double", new(OpeningMarker, ClosingMarker)) { 
        Value = value;
    }
}
