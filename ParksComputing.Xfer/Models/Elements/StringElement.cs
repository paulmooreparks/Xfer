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
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningMarker, ClosingMarker);

    public StringElement(string text, int markerCount = 1, ElementStyle style = ElementStyle.Normal) : 
        base(text, ElementName, new(OpeningMarker, ClosingMarker, markerCount, style)) 
    { 
    }
}
