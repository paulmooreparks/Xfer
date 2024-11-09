using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class DecimalElement : Element {
    public static readonly string ElementName = "decimal";
    public const char OpeningMarker = '*';
    public const char ClosingMarker = OpeningMarker;

    public decimal TypedValue { get; set; }
    public override string Value => TypedValue.ToString();

    public DecimalElement(decimal value)
        : base(ElementName, new Delimiter(OpeningMarker, ClosingMarker)) {
        TypedValue = value;
    }

    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append(Delimiter.Opening);
        sb.Append(Value);
        sb.Append(Delimiter.Closing);
        return sb.ToString();
    }
}
