using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a double-precision floating-point number element in XferLang using caret (^) delimiters.
/// Double elements store IEEE 754 double-precision floating-point values suitable for
/// scientific calculations and general-purpose numeric operations.
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

    /// <summary>
    /// Custom formatter function for the double value. If null, uses default formatting.
    /// </summary>
    public Func<double, string>? CustomFormatter { get; set; }

    /// <summary>
    /// Initializes a new instance of the DoubleElement class with the specified value and formatting options.
    /// </summary>
    /// <param name="value">The double-precision floating-point value to represent</param>
    /// <param name="specifierCount">The number of delimiter characters to use (default: 1)</param>
    /// <param name="style">The element style for delimiter handling (default: Compact)</param>
    /// <param name="customFormatter">Optional custom formatter function for the double value</param>
    public DoubleElement(double value, int specifierCount = 1, ElementStyle style = ElementStyle.Compact, Func<double, string>? customFormatter = null)
        : this(new NumericValue<double>(value), specifierCount, style, customFormatter) {
    }

    /// <summary>
    /// Initializes a new instance of the DoubleElement class with the specified numeric value.
    /// <param name="numericValue">The numeric value to represent as a double</param>
    /// <param name="specifierCount">The number of delimiter characters to use (default: 1)</param>
    /// <param name="style">The element style for delimiter handling (default: Compact)</param>
    /// <param name="customFormatter">Optional custom formatter function for the double value</param>
    /// </summary>
    public DoubleElement(NumericValue<double> numericValue, int specifierCount = 1, ElementStyle style = ElementStyle.Compact, Func<double, string>? customFormatter = null)
        : base(numericValue, ElementName, new EmptyClosingElementDelimiter(OpeningSpecifier, ClosingSpecifier, specifierCount, style))
    {
        CustomFormatter = customFormatter;
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
