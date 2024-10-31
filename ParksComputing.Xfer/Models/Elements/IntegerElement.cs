using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class IntegerElement : Element {
    public static readonly string ElementName = "integer";
    public const char OpeningMarker = '#';
    public const char ClosingMarker = OpeningMarker;

    public int Value { get; set; } = 0;

    public IntegerElement(int value) : base(ElementName, new(OpeningMarker, ClosingMarker)) { 
        Value = value;
    }
}
