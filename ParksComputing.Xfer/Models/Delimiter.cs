using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models;

public class Delimiter {
    public char Marker { get; }
    public int Count { get; set; }

    public string Opening { get; }
    public string Closing { get; }

    public Delimiter(char marker) : this(marker, 1) {
    }
    public Delimiter(char marker, int count) {
        if (count < 1) {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be at least 1.");
        }

        if (char.IsWhiteSpace(marker) || char.IsLetterOrDigit(marker)) {
            throw new ArgumentException("Marker cannot be an alphanumeric or whitespace character.", nameof(marker));
        }

        Marker = marker;
        Count = count;

        var repeatedMarker = new string(marker, count);
        Opening = "<" + repeatedMarker;
        Closing = repeatedMarker + ">";
    }

    public override string ToString() {
        return Opening;
    }
}
