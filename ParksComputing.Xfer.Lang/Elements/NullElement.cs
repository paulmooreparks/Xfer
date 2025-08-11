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
    /// <summary>Element name used for null literal.</summary>
    public static readonly string ElementName = "null";
    /// <summary>Opening specifier ('?').</summary>
    public const char OpeningSpecifier = '?';
    /// <summary>Closing specifier (same as opening).</summary>
    public const char ClosingSpecifier = OpeningSpecifier;
    /// <summary>Delimiter configuration for null literal.</summary>
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    /// <summary>Create a new null element.</summary>
    /// <param name="style">Delimiter style (implicit, compact, explicit).</param>
    public NullElement(ElementStyle style = ElementStyle.Compact)
        : base(null, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, 1, style))
    {
    }

    /// <inheritdoc />
    public override string ToXfer()
    {
        return ToXfer(Formatting.None);
    }

    /// <inheritdoc />
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0)
    {
        var sb = new StringBuilder();
        sb.Append($"{Delimiter.CompactOpening}");
        return sb.ToString();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "null";
    }
}
