using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class DoubleElement : NumericElement<double> {
    public static readonly string ElementName = "double";
    public const char OpeningMarker = '^';
    public const char ClosingMarker = OpeningMarker;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningMarker, ClosingMarker);

    public DoubleElement(double value, int markerCount = 1, ElementStyle style = ElementStyle.Compact) 
        : base(value, ElementName, new ElementDelimiter(OpeningMarker, ClosingMarker, markerCount, style)) {
    }
}
