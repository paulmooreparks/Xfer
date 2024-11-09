using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class StringElement : Element {
    public static readonly string ElementName = "string";
    public const char OpeningMarker = '"';
    public const char ClosingMarker = OpeningMarker;

    public string TypedValue { get; set; } = string.Empty;
    public override string Value => TypedValue;

    public StringElement(string text, int markerCount = 1) : base(ElementName, new(OpeningMarker, ClosingMarker, markerCount)) { 
        TypedValue = text;
    }

    public override string ToString() {
        return $"{Delimiter.Opening}{Value}{Delimiter.Closing}";
    }
}
