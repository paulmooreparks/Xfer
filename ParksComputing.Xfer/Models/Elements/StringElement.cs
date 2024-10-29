using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class StringElement : Element {
    public const char OpeningMarker = '"';
    public const char ClosingMarker = OpeningMarker;

    public string Value { get; set; } = string.Empty;

    public StringElement(string text, int markerCount = 1) : base("string", new(OpeningMarker, ClosingMarker, markerCount)) { 
        Value = text;
    }

    public override string ToString() {
        return $"{Delimiter.Opening}{Value}{Delimiter.Closing}";
    }
}
