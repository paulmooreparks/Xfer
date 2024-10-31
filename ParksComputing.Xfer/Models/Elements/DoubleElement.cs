using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class DoubleElement : Element {
    public static readonly string ElementName = "double";
    public const char OpeningMarker = '^';
    public const char ClosingMarker = OpeningMarker;

    public double Value { get; set; } = 0.0;

    public DoubleElement(double value) : base(ElementName, new(OpeningMarker, ClosingMarker)) { 
        Value = value;
    }
}
