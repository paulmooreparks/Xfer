using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParksComputing.Xfer.Lang.Attributes;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a 64-bit signed integer element in XferLang.
/// Uses ampersand (&) delimiters and supports attribute-driven hexadecimal or binary formatting.
/// </summary>
public class LongElement : NumericElement<long> {
    /// <summary>
    /// The element name used in XferLang serialization for long integers.
    /// </summary>
    public static readonly string ElementName = "longInteger";

    /// <summary>
    /// The opening delimiter character (ampersand) for long integer elements.
    /// </summary>
    public const char OpeningSpecifier = '&';

    /// <summary>
    /// The closing delimiter character (ampersand) for long integer elements.
    /// </summary>
    public const char ClosingSpecifier = OpeningSpecifier;

    /// <summary>
    /// The delimiter configuration for long integer elements using ampersand characters.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = new EmptyClosingElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    // Formatting metadata (set internally from attributes)
    private XferNumericFormat _format = XferNumericFormat.Decimal;
    private int _minBits;        // For binary padding
    private int _minDigits;      // For hex padding

    internal void SetNumericFormat(XferNumericFormat format, int minBits, int minDigits) {
        _format = format;
        _minBits = minBits;
        _minDigits = minDigits;
    }

    /// <summary>
    /// Initializes a new instance of the LongElement class with the specified value and formatting options.
    /// </summary>
    /// <param name="value">The long integer value to represent</param>
    /// <param name="specifierCount">The number of delimiter characters to use (default: 1)</param>
    /// <param name="style">The element style for delimiter handling (default: Compact)</param>
    public LongElement(long value, int specifierCount = 1, ElementStyle style = ElementStyle.Compact)
        : this(new NumericValue<long>(value), specifierCount, style) {
    }

    /// <summary>
    /// Initializes a new instance of the LongElement class with the specified numeric value.
    /// </summary>
    public LongElement(NumericValue<long> numericValue, int specifierCount = 1, ElementStyle style = ElementStyle.Compact)
        : base(numericValue, ElementName, new EmptyClosingElementDelimiter(OpeningSpecifier, ClosingSpecifier, specifierCount, style)) {
    }

    /// <summary>
    /// Serializes this long integer element to its XferLang string representation.
    /// Uses ampersand delimiters and applies custom formatting if specified.
    /// </summary>
    /// <param name="formatting">The formatting style to apply during serialization</param>
    /// <param name="indentChar">The character to use for indentation (default: space)</param>
    /// <param name="indentation">The number of indentation characters per level (default: 2)</param>
    /// <param name="depth">The current nesting depth for indentation calculation (default: 0)</param>
    /// <returns>The XferLang string representation of this long integer element</returns>
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        var sb = new StringBuilder();
        string valueString;
        switch (_format) {
            case XferNumericFormat.Hexadecimal:
                valueString = Helpers.NumericFormatter.FormatLong(Value, XferNumericFormat.Hexadecimal, 0, _minDigits);
                break;
            case XferNumericFormat.Binary:
                valueString = Helpers.NumericFormatter.FormatLong(Value, XferNumericFormat.Binary, _minBits, 0);
                break;
            default:
                valueString = Value.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
                break;
        }

        if (Delimiter.Style == ElementStyle.Compact) {
            sb.Append($"{Delimiter.CompactOpening}{valueString}{Delimiter.CompactClosing}");
        }
        else {
            sb.Append($"{Delimiter.ExplicitOpening}{valueString}{Delimiter.ExplicitClosing}");
        }

        return sb.ToString();
    }
}
