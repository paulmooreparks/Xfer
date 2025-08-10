using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;
/// <summary>
/// Represents a null literal element in XferLang. Public so scripting tests can construct it.
/// </summary>
public class NullElement : TypedElement<object?>
{
    public static readonly string ElementName = "null";
    public const char OpeningSpecifier = '?';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    public NullElement(ElementStyle style = ElementStyle.Compact)
        : base(null, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, 1, style))
    {
    }

    public override string ToXfer()
    {
        return ToXfer(Formatting.None);
    }

    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0)
    {
        var sb = new StringBuilder();
        sb.Append($"{Delimiter.CompactOpening}");
        return sb.ToString();
    }

    public override string ToString()
    {
        return "null";
    }
}
