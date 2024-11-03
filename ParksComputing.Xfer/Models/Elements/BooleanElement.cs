using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class BooleanElement : Element {
    public static readonly string ElementName = "boolean";
    public const char OpeningMarker = '~';
    public const char ClosingMarker = OpeningMarker;

    public bool Value { get; set; }

    public BooleanElement(bool value)
        : base(ElementName, new Delimiter(OpeningMarker, ClosingMarker)) {
        Value = value;
    }

    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append(Delimiter.Opening);
        sb.Append(Value ? "true" : "false");
        sb.Append(Delimiter.Closing);
        return sb.ToString();
    }
}
