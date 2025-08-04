using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a character element in XferLang using backslash (\) delimiters.
/// Character elements store Unicode code points as integers and can represent
/// any valid Unicode character. The value represents the numeric code point
/// (0 to 0x10FFFF) rather than the character itself.
/// </summary>
public class CharacterElement : TypedElement<int> {
    /// <summary>
    /// The element type name for character elements.
    /// </summary>
    public static readonly string ElementName = "character";

    /// <summary>
    /// The opening delimiter character for character elements.
    /// </summary>
    public const char OpeningSpecifier = '\\';

    /// <summary>
    /// The closing delimiter character for character elements.
    /// </summary>
    public const char ClosingSpecifier = OpeningSpecifier;

    /// <summary>
    /// The default element delimiter configuration for character elements.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    /// <summary>
    /// Initializes a new CharacterElement with the specified Unicode code point and formatting options.
    /// </summary>
    /// <param name="codePoint">The Unicode code point to store (0 to 0x10FFFF).</param>
    /// <param name="specifierCount">The number of delimiter characters to use (default: 1).</param>
    /// <param name="style">The element style for formatting (default: Compact).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the code point is outside the valid Unicode range.</exception>
    public CharacterElement(int codePoint, int specifierCount = 1, ElementStyle style = ElementStyle.Compact) :
        base(codePoint, ElementName, new(OpeningSpecifier, ClosingSpecifier, specifierCount, style)) {
        if (codePoint < 0 || codePoint > 0x10FFFF) {
            throw new ArgumentOutOfRangeException(nameof(codePoint), "Code point must be between 0 and 0x10FFFF.");
        }
    }

    /// <summary>
    /// Converts the character element to its XferLang string representation without formatting.
    /// </summary>
    /// <returns>The XferLang string representation of the Unicode code point.</returns>
    public override string ToXfer() {
        return ToXfer(Formatting.None);
    }

    /// <summary>
    /// Converts the character element to its XferLang string representation with formatting options.
    /// </summary>
    /// <param name="formatting">The formatting style to apply.</param>
    /// <param name="indentChar">The character to use for indentation.</param>
    /// <param name="indentation">The number of indent characters per level.</param>
    /// <param name="depth">The current indentation depth.</param>
    /// <returns>The formatted XferLang string representation of the Unicode code point.</returns>
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        var sb = new StringBuilder();
        sb.Append($"{Delimiter.MinOpening}${Value:X} ");
        return sb.ToString();
    }

    /// <summary>
    /// Returns the Unicode character represented by this code point.
    /// </summary>
    /// <returns>The Unicode character as a string.</returns>
    public override string ToString() {
        return char.ConvertFromUtf32(Value);
    }
}
