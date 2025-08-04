using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a 32-bit signed integer element in XferLang.
/// Uses hash (#) delimiters and supports custom formatting for integer values.
/// </summary>
public class IntegerElement : NumericElement<int>
{
    /// <summary>
    /// The element name used in XferLang serialization for integers.
    /// </summary>
    public static readonly string ElementName = "integer";

    /// <summary>
    /// The opening delimiter character (hash) for integer elements.
    /// </summary>
    public const char OpeningSpecifier = '#';

    /// <summary>
    /// The closing delimiter character (hash) for integer elements.
    /// </summary>
    public const char ClosingSpecifier = OpeningSpecifier;

    /// <summary>
    /// The delimiter configuration for integer elements using hash characters.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    /// <summary>
    /// Custom formatter function for the integer value. If null, uses default formatting.
    /// </summary>
    public Func<int, string>? CustomFormatter { get; set; }

    /// <summary>
    /// Initializes a new instance of the IntegerElement class with the specified value and formatting options.
    /// </summary>
    /// <param name="value">The integer value to represent</param>
    /// <param name="specifierCount">The number of delimiter characters to use (default: 1)</param>
    /// <param name="elementStyle">The element style for delimiter handling (default: Compact)</param>
    /// <param name="customFormatter">Optional custom formatter function for the integer value</param>
    public IntegerElement(int value, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact, Func<int, string>? customFormatter = null)
        : base(value, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, specifierCount, elementStyle))
    {
        CustomFormatter = customFormatter;
    }

    /// <summary>
    /// Serializes this integer element to its XferLang string representation.
    /// Uses hash delimiters and applies custom formatting if specified.
    /// </summary>
    /// <param name="formatting">The formatting style to apply during serialization</param>
    /// <param name="indentChar">The character to use for indentation (default: space)</param>
    /// <param name="indentation">The number of indentation characters per level (default: 2)</param>
    /// <param name="depth">The current nesting depth for indentation calculation (default: 0)</param>
    /// <returns>The XferLang string representation of this integer element</returns>
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
