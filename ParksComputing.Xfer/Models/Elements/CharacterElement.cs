using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class CharacterElement : TypedElement<char> {
    public static readonly string ElementName = "character";
    public const char OpeningSpecifier = '\\';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    public int CharValue => Value;

    public CharacterElement(char ch, int specifierCount = 1, ElementStyle style = ElementStyle.Normal) : 
        base(ch, ElementName, new(OpeningSpecifier, ClosingSpecifier, specifierCount, style)) {
    }

    public override string ToString() {
        return $"{Delimiter.MinOpening}{CharValue} ";
    }
}
