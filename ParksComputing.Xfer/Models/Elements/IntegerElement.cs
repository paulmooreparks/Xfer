using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class IntegerElement : Element {
    public int Value { get; set; } = 0;

    public const char OpeningMarker = '#';
    public const char ClosingMarker = OpeningMarker;

    public IntegerElement(int value) : base("integer", new(OpeningMarker, ClosingMarker)) { 
        Value = value;
    }
}
