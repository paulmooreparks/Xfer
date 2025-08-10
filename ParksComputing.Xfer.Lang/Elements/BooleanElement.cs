using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a boolean element in XferLang using tilde (~) delimiters.
/// Boolean elements store true/false values and are rendered as "true" or "false"
/// in the XferLang format. The compact style uses ~ delimiters by default.
/// </summary>
public class BooleanElement : TypedElement<bool> {
    /// <summary>
    /// The element type name for boolean elements.
    /// </summary>
    public static readonly string ElementName = "boolean";

    /// <summary>
    /// The opening delimiter character for boolean elements.
    /// </summary>
    public const char OpeningSpecifier = '~';

    /// <summary>
    /// The closing delimiter character for boolean elements.
    /// </summary>
    public const char ClosingSpecifier = OpeningSpecifier;

    /// <summary>
    /// The default element delimiter configuration for boolean elements.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = new EmptyClosingElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    /// <summary>
    /// The string representation of the true value.
    /// </summary>
    public static readonly string TrueValue = "true";

    /// <summary>
    /// The string representation of the false value.
    /// </summary>
    public static readonly string FalseValue = "false";

    /// <summary>
    /// Initializes a new BooleanElement with the specified value and formatting options.
    /// </summary>
    /// <param name="value">The boolean value to store.</param>
    /// <param name="specifierCount">The number of delimiter characters to use (default: 1).</param>
    /// <param name="style">The element style for formatting (default: Compact).</param>
    public BooleanElement(bool value, int specifierCount = 1, ElementStyle style = ElementStyle.Compact)
        : base(value, ElementName, new EmptyClosingElementDelimiter(OpeningSpecifier, ClosingSpecifier, specifierCount, style)) {
    }

    /// <summary>
    /// Converts the boolean element to its XferLang string representation without formatting.
    /// </summary>
    /// <returns>The XferLang string representation of the boolean value.</returns>
    public override string ToXfer() {
        return ToXfer(Formatting.None);
    }

    /// <summary>
    /// Converts the boolean element to its XferLang string representation with formatting options.
    /// </summary>
    /// <param name="formatting">The formatting style to apply.</param>
    /// <param name="indentChar">The character to use for indentation.</param>
    /// <param name="indentation">The number of indent characters per level.</param>
    /// <param name="depth">The current indentation depth.</param>
    /// <returns>The formatted XferLang string representation of the boolean value.</returns>
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        var value = Value ? TrueValue : FalseValue;
        var sb = new StringBuilder();

        if (Delimiter.Style == ElementStyle.Compact) {
            sb.Append($"{Delimiter.OpeningSpecifier}{value}{Delimiter.CompactClosing}{Delimiter.CompactClosing}");
        }
        else {
            sb.Append($"{Delimiter.ExplicitOpening}{value}{Delimiter.ExplicitClosing}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns the string representation of the boolean value.
    /// </summary>
    /// <returns>The boolean value as a string.</returns>
    public override string ToString() {
        return Value.ToString();
    }
}
