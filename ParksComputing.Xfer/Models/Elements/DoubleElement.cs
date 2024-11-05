using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class DoubleElement : Element {
    public static readonly string ElementName = "double";
    public const char OpeningMarker = '^';
    public const char ClosingMarker = OpeningMarker;

    public double Value { get; set; }

    public DoubleElement(double value)
        : base(ElementName, new Delimiter(OpeningMarker, ClosingMarker)) {
        Value = value;
    }

    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append(Delimiter.Opening);
        sb.Append(Value);
        sb.Append(Delimiter.Closing);
        return sb.ToString();
    }
}
