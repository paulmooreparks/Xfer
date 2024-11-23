using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class PlaceholderElement : TextElement {
    public static readonly string ElementName = "placeholder";
    public const char OpeningMarker = '|';
    public const char ClosingMarker = OpeningMarker;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningMarker, ClosingMarker);

    public PlaceholderElement(string text, int markerCount = 1, ElementStyle style = ElementStyle.Normal) 
        : base(text, ElementName, new(OpeningMarker, ClosingMarker, markerCount, style)) 
    {
    }
}
