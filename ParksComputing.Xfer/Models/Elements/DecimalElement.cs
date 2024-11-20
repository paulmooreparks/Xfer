using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class DecimalElement : TypedElement<decimal> {
    public static readonly string ElementName = "decimal";
    public const char OpeningMarker = '*';
    public const char ClosingMarker = OpeningMarker;
    public static readonly Delimiter ElementDelimiter = new Delimiter(OpeningMarker, ClosingMarker);

    public DecimalElement(decimal value, int markerCount = 1) : base(value, ElementName, new Delimiter(OpeningMarker, ClosingMarker, markerCount)) {
    }

    public override string ToString() {
        return $"{Delimiter.MinOpening}{Value} ";
    }
}
