using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParksComputing.Xfer.Lang.Attributes;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a double-precision floating-point number element in XferLang using caret (^) delimiters.
/// Attribute metadata can control precision, trailing zeros, and alternative bases (hex/binary).
/// </summary>
public class DoubleElement : NumericElement<double>
{
    /// <summary>
    /// The element name used in XferLang serialization for double-precision numbers.
    /// </summary>
    public static readonly string ElementName = "double";

    /// <summary>
    /// The opening delimiter character (caret) for double elements.
    /// </summary>
    public const char OpeningSpecifier = '^';

    /// <summary>
    /// The closing delimiter character (caret) for double elements.
    /// </summary>
    public const char ClosingSpecifier = OpeningSpecifier;

    /// <summary>
    /// The delimiter configuration for double elements using caret characters.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = new EmptyClosingElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    // Formatting metadata
    private XferNumericFormat _format = XferNumericFormat.Decimal;
    private int _minBits;
    private int _minDigits;
    private int? _precision;
    private bool _removeTrailingZeros = true;

    internal void SetNumericFormat(XferNumericFormat format, int minBits, int minDigits) {
        _format = format;
        _minBits = minBits;
        _minDigits = minDigits;
    }

    internal void SetPrecision(int? precision, bool removeTrailingZeros) {
        _precision = precision;
        _removeTrailingZeros = removeTrailingZeros;
    }

    /// <summary>
    /// Initializes a new instance of the DoubleElement class with the specified value and formatting options.
    /// </summary>
    /// <param name="value">The double-precision floating-point value to represent</param>
    /// <param name="specifierCount">The number of delimiter characters to use (default: 1)</param>
    /// <param name="style">The element style for delimiter handling (default: Compact)</param>
    public DoubleElement(double value, int specifierCount = 1, ElementStyle style = ElementStyle.Compact)
        : this(new NumericValue<double>(value), specifierCount, style) {
    }

    /// <summary>
    /// Initializes a new instance of the DoubleElement class with the specified numeric value.
    /// <param name="numericValue">The numeric value to represent as a double</param>
    /// <param name="specifierCount">The number of delimiter characters to use (default: 1)</param>
    /// <param name="style">The element style for delimiter handling (default: Compact)</param>
    /// </summary>
    public DoubleElement(NumericValue<double> numericValue, int specifierCount = 1, ElementStyle style = ElementStyle.Compact)
        : base(numericValue, ElementName, new EmptyClosingElementDelimiter(OpeningSpecifier, ClosingSpecifier, specifierCount, style))
    {
    }

    /// <summary>
    /// Serializes this double element to its XferLang string representation.
    /// Uses caret delimiters and applies custom formatting if specified.
    /// </summary>
    /// <param name="formatting">The formatting style to apply during serialization</param>
    /// <param name="indentChar">The character to use for indentation (default: space)</param>
    /// <param name="indentation">The number of indentation characters per level (default: 2)</param>
    /// <param name="depth">The current nesting depth for indentation calculation (default: 0)</param>
    /// <returns>The XferLang string representation of this double element</returns>
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0)
    {
        var sb = new StringBuilder();
        string valueString;
        switch (_format) {
            case XferNumericFormat.Hexadecimal:
                valueString = Helpers.NumericFormatter.FormatDouble(Value, XferNumericFormat.Hexadecimal, 0, _minDigits);
                break;
            case XferNumericFormat.Binary:
                valueString = Helpers.NumericFormatter.FormatDouble(Value, XferNumericFormat.Binary, _minBits, 0);
                break;
            default:
                if (_precision.HasValue) {
                    var rounded = Math.Round(Value, _precision.Value, MidpointRounding.AwayFromZero);
                    string raw = rounded.ToString($"F{_precision.Value}", System.Globalization.CultureInfo.InvariantCulture);
                    if (_removeTrailingZeros && _precision.Value > 0) {
                        raw = raw.TrimEnd('0').TrimEnd('.');
                    }
                    valueString = raw;
                } else {
                    valueString = Value.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
                }
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
