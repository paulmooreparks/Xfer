using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class BooleanElement : TypedElement<bool> {
    public static readonly string ElementName = "boolean";
    public const char OpeningMarker = '~';
    public const char ClosingMarker = OpeningMarker;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningMarker, ClosingMarker);

    public static readonly string TrueValue = "true";
    public static readonly string FalseValue = "false";

    public BooleanElement(bool value, int markerCount = 1, ElementStyle style = ElementStyle.Normal)
        : base(value, ElementName, new ElementDelimiter(OpeningMarker, ClosingMarker, markerCount, style)) {
    }

    public override string ToString() {
        var value = Value ? TrueValue : FalseValue;

        if (Delimiter.Style == ElementStyle.Bare) {
            return $"{value} ";
        }
        if (Delimiter.Style == ElementStyle.Minimized) {
            return $"{Delimiter.OpeningMarker}{value} ";
        }
        return $"{Delimiter.Opening}{value}{Delimiter.Closing}";
    }
}
