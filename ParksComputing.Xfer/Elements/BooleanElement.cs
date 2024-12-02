using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Elements;

public class BooleanElement : TypedElement<bool>
{
    public static readonly string ElementName = "boolean";
    public const char OpeningSpecifier = '~';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    public static readonly string TrueValue = "true";
    public static readonly string FalseValue = "false";

    public BooleanElement(bool value, int specifierCount = 1, ElementStyle style = ElementStyle.Compact)
        : base(value, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, specifierCount, style))
    {
    }

    public override string ToXfer()
    {
        return ToXfer(Formatting.None);
    }

    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0)
    {
        var value = Value ? TrueValue : FalseValue;
        var sb = new StringBuilder();

        if (Delimiter.Style == ElementStyle.Implicit)
        {
            sb.Append(value);
        }
        else if (Delimiter.Style == ElementStyle.Compact)
        {
            sb.Append($"{Delimiter.OpeningSpecifier}{value} ");
        }
        else
        {
            sb.Append($"{Delimiter.Opening}{value}{Delimiter.Closing}");
        }

        return sb.ToString();
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
