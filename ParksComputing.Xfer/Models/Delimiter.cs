using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models;

public class Delimiter {
    public char OpeningMarker { get; }
    public char ClosingMarker { get; }
    public int MarkerCount { get; set; }
    public bool IsMinimized { get; } = false;

    public string Opening { get; }
    public string Closing { get; }

    public string MinOpening { get; }

    public Delimiter() : this(default, default, 1) { }

    public Delimiter(int markerCount) : this(default, default, markerCount) {
    }

    public Delimiter(char openingMarker, char closingMarker, bool isMinimized = false) : this(openingMarker, closingMarker, 1, isMinimized) {
    }

    public Delimiter(char openingMarker, char closingMarker, int markerCount, bool isMinimized = false) {
        if (markerCount < 1) {
            throw new ArgumentOutOfRangeException(nameof(markerCount), "Count must be at least 1.");
        }

        ValidateMarker(openingMarker, nameof(openingMarker));
        ValidateMarker(closingMarker, nameof(closingMarker));

        OpeningMarker = openingMarker;
        ClosingMarker = closingMarker;
        MarkerCount = markerCount;
        IsMinimized = isMinimized;

        var repeatedOpening = new string(openingMarker, markerCount);
        var repeatedClosing = new string(closingMarker, markerCount);

        Opening = "<" + repeatedOpening;
        Closing = repeatedClosing + ">";
        MinOpening = repeatedOpening;
    }

    private static void ValidateMarker(char marker, string paramName) {
        if (char.IsWhiteSpace(marker) || char.IsLetterOrDigit(marker)) {
            throw new ArgumentException("Marker cannot be an alphanumeric or whitespace character.", paramName);
        }
    }

    public override string ToString() {
        return $"{Opening}...{Closing}";
    }
}
