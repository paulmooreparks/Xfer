using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class CharacterElement : Element {
    public static readonly string ElementName = "character";
    public const char OpeningMarker = '\\';
    public const char ClosingMarker = OpeningMarker;

    public char TypedValue { get; set; } = default;
    public override string Value => TypedValue.ToString();

    public CharacterElement(char ch, int markerCount = 1) : base(ElementName, new(OpeningMarker, ClosingMarker, markerCount)) {
        TypedValue = ch;
    }

    public override string ToString() {
        return $"{Delimiter.Opening}{Value}{Delimiter.Closing}";
    }
}
