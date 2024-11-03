using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class BinaryElement : Element {
    public static readonly string ElementName = "binary";
    public const char OpeningMarker = '%';
    public const char ClosingMarker = OpeningMarker;

    public long Value { get; set; }

    public BinaryElement(int value)
        : base(ElementName, new Delimiter(OpeningMarker, ClosingMarker)) {
        Value = value;
    }

    public BinaryElement(byte[] bytes)
        : base(ElementName, new Delimiter(OpeningMarker, ClosingMarker)) {
        Value = BitConverter.ToInt64(bytes, 0);
    }

    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append(Delimiter.Opening);
        sb.Append(Convert.ToString(Value, 2));
        sb.Append(Delimiter.Closing);
        return sb.ToString();
    }
}
