using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Elements;

public class CharacterElement : TypedElement<int>
{
    public static readonly string ElementName = "character";
    public const char OpeningSpecifier = '\\';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    public CharacterElement(int codePoint, int specifierCount = 1, ElementStyle style = ElementStyle.Compact) :
        base(codePoint, ElementName, new(OpeningSpecifier, ClosingSpecifier, specifierCount, style))
    {
        if (codePoint < 0 || codePoint > 0x10FFFF)
        {
            throw new ArgumentOutOfRangeException(nameof(codePoint), "Code point must be between 0 and 0x10FFFF.");
        }
    }

    public override string ToXfer()
    {
        return ToXfer(Formatting.None);
    }

    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0)
    {
        var sb = new StringBuilder();
        sb.Append($"{Delimiter.MinOpening}${Value:X} ");
        return sb.ToString();
    }

    public override string ToString()
    {
        return char.ConvertFromUtf32(Value);
    }
}
