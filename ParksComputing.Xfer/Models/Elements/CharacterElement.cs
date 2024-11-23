using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class CharacterElement : TypedElement<char> {
    public static readonly string ElementName = "character";
    public const char OpeningMarker = '\\';
    public const char ClosingMarker = OpeningMarker;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningMarker, ClosingMarker);

    public int CharValue => Value;

    public CharacterElement(char ch, int markerCount = 1, ElementStyle style = ElementStyle.Normal) : 
        base(ch, ElementName, new(OpeningMarker, ClosingMarker, markerCount, style)) {
    }

    public override string ToString() {
        return $"{Delimiter.MinOpening}{CharValue} ";
    }
}
