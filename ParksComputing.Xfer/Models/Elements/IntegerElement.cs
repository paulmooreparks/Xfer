using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class IntegerElement : TypedElement<int> {
    public static readonly string ElementName = "integer";
    public const char OpeningMarker = '#';
    public const char ClosingMarker = OpeningMarker;
    public static readonly Delimiter ElementDelimiter = new Delimiter(OpeningMarker, ClosingMarker);

    public IntegerElement(int value, int markerCount = 1)
        : base(value, ElementName, new Delimiter(OpeningMarker, ClosingMarker, markerCount)) {
    }

    public override string ToString() {
        return $"{Delimiter.MinOpening}{Value} ";
    }
}
