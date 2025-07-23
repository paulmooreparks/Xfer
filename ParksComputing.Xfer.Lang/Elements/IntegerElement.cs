using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

public class IntegerElement : NumericElement<int>
{
    public static readonly string ElementName = "integer";
    public const char OpeningSpecifier = '#';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    /// <summary>
    /// Custom formatter function for the integer value. If null, uses default formatting.
    /// </summary>
    public Func<int, string>? CustomFormatter { get; set; }

    public IntegerElement(int value, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact, Func<int, string>? customFormatter = null)
        : base(value, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, specifierCount, elementStyle))
    {
        CustomFormatter = customFormatter;
    }

    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0)
    {
        var sb = new StringBuilder();
        string valueString = CustomFormatter?.Invoke(Value) ?? Value.ToString();

        if (Delimiter.Style == ElementStyle.Implicit)
        {
            sb.Append($"{valueString} ");
        }
        else if (Delimiter.Style == ElementStyle.Compact)
        {
            // For custom formatted values (hex/binary), they already include the # prefix
            if (CustomFormatter != null && valueString.StartsWith("#"))
            {
                sb.Append($"{valueString} ");
            }
            else
            {
                sb.Append($"{Delimiter.OpeningSpecifier}{valueString} ");
            }
        }
        else
        {
            sb.Append($"{Delimiter.Opening}{valueString}{Delimiter.Closing}");
        }

        return sb.ToString();
    }
}
