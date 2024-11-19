using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models;

public class Delimiter {
    public char OpeningMarker { get; }
    public char ClosingMarker { get; }
    public int Count { get; set; }

    public string Opening { get; }
    public string Closing { get; }

    public Delimiter() : this(default, default, 1) { }

    public Delimiter(int markerCount) : this(default, default, markerCount) {
    }

    public Delimiter(char openingMarker, char closingMarker) : this(openingMarker, closingMarker, 1) {
    }

    public Delimiter(char openingMarker, char closingMarker, int count) {
        if (count < 1) {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be at least 1.");
        }

        ValidateMarker(openingMarker, nameof(openingMarker));
        ValidateMarker(closingMarker, nameof(closingMarker));

        OpeningMarker = openingMarker;
        ClosingMarker = closingMarker;
        Count = count;

        var repeatedOpening = new string(openingMarker, count);
        var repeatedClosing = new string(closingMarker, count);

        Opening = "<" + repeatedOpening;
        Closing = repeatedClosing + ">";
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
