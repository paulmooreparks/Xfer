using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class StringElement : TextElement {
    public static readonly string ElementName = "string";
    public const char OpeningMarker = '"';
    public const char ClosingMarker = OpeningMarker;

    public StringElement(string text, int markerCount = 1) : base(text, ElementName, new(OpeningMarker, ClosingMarker, markerCount)) { 
    }
}
