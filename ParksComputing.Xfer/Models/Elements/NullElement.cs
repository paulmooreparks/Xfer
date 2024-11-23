using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;
internal class NullElement : TypedElement<object?> {
    public static readonly string ElementName = "null";
    public const char OpeningMarker = '?';
    public const char ClosingMarker = OpeningMarker;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningMarker, ClosingMarker);

    public NullElement(ElementStyle style = ElementStyle.Minimized)
        : base(null, ElementName, new ElementDelimiter(OpeningMarker, ClosingMarker, 1, style)) {
    }

    public override string ToString() {
        return $"{Delimiter.MinOpening} ";
    }
}
