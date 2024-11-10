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

    public static readonly string TrueValue = "true";
    public static readonly string FalseValue = "false";

    public BooleanElement(bool value)
        : base(value, ElementName, new Delimiter(OpeningMarker, ClosingMarker)) {
    }

    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append(Delimiter.Opening);
        sb.Append(Value ? TrueValue : FalseValue);
        sb.Append(Delimiter.Closing);
        return sb.ToString();
    }
}
