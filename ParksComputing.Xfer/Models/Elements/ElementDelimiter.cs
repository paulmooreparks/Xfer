using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class ElementDelimiter
{
    public char OpeningMarker { get; }
    public char ClosingMarker { get; }
    public int MarkerCount { get; set; }
    public ElementStyle Style { get; } = ElementStyle.Normal;

    public string Opening { get; }
    public string Closing { get; }

    public string MinOpening { get; }

    public ElementDelimiter() : this(default, default, 1) { }

    public ElementDelimiter(int markerCount) : this(default, default, markerCount)
    {
    }

    public ElementDelimiter(char openingMarker, char closingMarker, ElementStyle elementStyle = ElementStyle.Normal) : this(openingMarker, closingMarker, 1, elementStyle)
    {
    }

    public ElementDelimiter(char openingMarker, char closingMarker, int markerCount, ElementStyle style = ElementStyle.Normal)
    {
        if (markerCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(markerCount), "Count must be at least 1.");
        }

        ValidateMarker(openingMarker, nameof(openingMarker));
        ValidateMarker(closingMarker, nameof(closingMarker));

        OpeningMarker = openingMarker;
        ClosingMarker = closingMarker;
        MarkerCount = markerCount;
        Style = style;

        var repeatedOpening = new string(openingMarker, markerCount);
        var repeatedClosing = new string(closingMarker, markerCount);

        Opening = "<" + repeatedOpening;
        Closing = repeatedClosing + ">";
        MinOpening = repeatedOpening;
    }

    private static void ValidateMarker(char marker, string paramName)
    {
        if (char.IsWhiteSpace(marker) || char.IsLetterOrDigit(marker))
        {
            throw new ArgumentException("Marker cannot be an alphanumeric or whitespace character.", paramName);
        }
    }

    public override string ToString()
    {
        return $"{Opening}...{Closing}";
    }
}
