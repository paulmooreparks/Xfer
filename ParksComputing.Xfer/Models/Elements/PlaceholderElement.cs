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
    public static readonly Delimiter ElementDelimiter = new Delimiter(OpeningMarker, ClosingMarker);

    public PlaceholderElement(string text, int markerCount = 1) : base(text, ElementName, new(OpeningMarker, ClosingMarker, markerCount)) {
    }
}
