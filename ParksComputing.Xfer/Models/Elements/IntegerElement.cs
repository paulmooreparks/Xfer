using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class IntegerElement : NumericElement<int> {
    public static readonly string ElementName = "integer";
    public const char OpeningMarker = '#';
    public const char ClosingMarker = OpeningMarker;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningMarker, ClosingMarker);

    public IntegerElement(int value, int markerCount = 1, ElementStyle elementStyle = ElementStyle.Normal)
        : base(value, ElementName, new ElementDelimiter(OpeningMarker, ClosingMarker, markerCount, elementStyle)) {
    }
}
