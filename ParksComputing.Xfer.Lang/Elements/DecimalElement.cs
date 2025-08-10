using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a decimal number element in XferLang using asterisk (*) delimiters.
/// Decimal elements store high-precision decimal values suitable for financial
/// calculations and other applications requiring exact decimal representation.
/// </summary>
public class DecimalElement : NumericElement<decimal> {
    /// <summary>
    /// The element name used in XferLang serialization for decimal numbers.
    /// </summary>
    public static readonly string ElementName = "decimal";

    /// <summary>
    /// The opening delimiter character (asterisk) for decimal elements.
    /// </summary>
    public const char OpeningSpecifier = '*';

    /// <summary>
    /// The closing delimiter character (asterisk) for decimal elements.
    /// </summary>
    public const char ClosingSpecifier = OpeningSpecifier;

    /// <summary>
    /// The delimiter configuration for decimal elements using asterisk characters.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = new EmptyClosingElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    /// <summary>
    /// Custom formatter function for the decimal value. If null, uses default formatting.
    /// </summary>
    public Func<decimal, string>? CustomFormatter { get; set; }

    /// <summary>
    /// Initializes a new instance of the DecimalElement class with the specified value and formatting options.
    /// </summary>
    /// <param name="value">The decimal value to represent</param>
    /// <param name="specifierCount">The number of delimiter characters to use (default: 1)</param>
    /// <param name="style">The element style for delimiter handling (default: Compact)</param>
    /// <param name="customFormatter">Optional custom formatter function for the decimal value</param>
    public DecimalElement(decimal value, int specifierCount = 1, ElementStyle style = ElementStyle.Compact, Func<decimal, string>? customFormatter = null)
        : this(new NumericValue<decimal>(value), specifierCount, style, customFormatter) {
    }

    public DecimalElement(NumericValue<decimal> numericValue, int specifierCount = 1, ElementStyle style = ElementStyle.Compact, Func<decimal, string>? customFormatter = null)
        : base(numericValue, ElementName, new EmptyClosingElementDelimiter(OpeningSpecifier, ClosingSpecifier, specifierCount, style)) {
        CustomFormatter = customFormatter;
    }

    /// <summary>
    /// Serializes this decimal element to its XferLang string representation.
    /// Uses asterisk delimiters and applies custom formatting if specified.
    /// </summary>
    /// <param name="formatting">The formatting style to apply during serialization</param>
    /// <param name="indentChar">The character to use for indentation (default: space)</param>
    /// <param name="indentation">The number of indentation characters per level (default: 2)</param>
    /// <param name="depth">The current nesting depth for indentation calculation (default: 0)</param>
    /// <returns>The XferLang string representation of this decimal element</returns>
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        var sb = new StringBuilder();
        string valueString = CustomFormatter != null ? CustomFormatter(Value) : NumericValue.ToString();

        if (Delimiter.Style == ElementStyle.Compact) {
            sb.Append($"{Delimiter.CompactOpening}{valueString}{Delimiter.CompactClosing}");
        }
        else {
            sb.Append($"{Delimiter.ExplicitOpening}{valueString}{Delimiter.ExplicitClosing}");
        }

        return sb.ToString();
    }
}
