using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class LongElement : TypedElement<long> {
    public static readonly string ElementName = "longInteger";
    public const char OpeningMarker = '&';
    public const char ClosingMarker = OpeningMarker;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningMarker, ClosingMarker);

    public LongElement(long value, int markerCount = 1, ElementStyle style = ElementStyle.Normal)
        : base(value, ElementName, new ElementDelimiter(OpeningMarker, ClosingMarker, markerCount, style)) {
    }

    public override string ToString() {
        return $"{Delimiter.MinOpening}{Value} ";
    }
}
