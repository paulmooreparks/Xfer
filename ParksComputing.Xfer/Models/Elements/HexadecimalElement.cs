using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class HexadecimalElement : Element {
    public static readonly string ElementName = "hexadecimal";
    public const char OpeningMarker = '$';
    public const char ClosingMarker = OpeningMarker;

    public int Value { get; set; }

    public HexadecimalElement(int value)
        : base(ElementName, new Delimiter(OpeningMarker, ClosingMarker)) {
        Value = value;
    }

    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append(Delimiter.Opening);
        sb.Append(Value.ToString("X"));
        sb.Append(Delimiter.Closing);
        return sb.ToString();
    }
}
