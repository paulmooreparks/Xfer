using System.Text;
using System.Text.RegularExpressions;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents an identifier element in XferLang using colon (:) delimiters.
/// Identifiers are used for variable names, property keys, and other symbolic references.
/// They follow standard identifier naming rules (letters, digits, underscores, hyphens, dots)
/// but must start with a letter or underscore.
/// </summary>
public class IdentifierElement : TextElement {
    /// <summary>
    /// The element name used in XferLang serialization for identifiers.
    /// </summary>
    public static readonly string ElementName = "identifier";

    /// <summary>
    /// The opening delimiter character (colon) for identifier elements.
    /// </summary>
    public const char OpeningSpecifier = ':';

    /// <summary>
    /// The closing delimiter character (colon) for identifier elements.
    /// </summary>
    public const char ClosingSpecifier = OpeningSpecifier;

    /// <summary>
    /// The delimiter configuration for identifier elements using colon characters.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    /// <summary>
    /// Initializes a new instance of the IdentifierElement class with the specified text and formatting options.
    /// </summary>
    /// <param name="text">The identifier text</param>
    /// <param name="specifierCount">The number of delimiter characters to use (default: 1)</param>
    /// <param name="style">The element style for delimiter handling (default: Implicit)</param>
    public IdentifierElement(string text, int specifierCount = 1, ElementStyle style = ElementStyle.Implicit) :
        base(text, ElementName, new(OpeningSpecifier, ClosingSpecifier, specifierCount, style)) {
    }

    /// <summary>
    /// Determines whether the specified character is valid as the first character of an identifier.
    /// Identifiers must start with a letter or underscore.
    /// </summary>
    /// <param name="c">The character to check</param>
    /// <returns>True if the character can start an identifier, false otherwise</returns>
    public static bool IsIdentifierLeadingChar(char c) {
        return char.IsLetter(c) || c == '_';
    }

    /// <summary>
    /// Checks and updates the delimiter style based on the identifier content.
    /// Uses implicit style for valid identifiers, compact for invalid characters, and explicit for edge cases.
    /// </summary>
    protected override void CheckAndUpdateDelimiterStyle() {
        int maxConsecutiveSpecifiers = GetMaxConsecutiveSpecifiers(Value, Delimiter.ClosingSpecifier);
        Delimiter.SpecifierCount = maxConsecutiveSpecifiers + 1;

        if (!Regex.IsMatch(Value, @"^[A-Za-z_\-\.][A-Za-z0-9_\-\.]*$")) {
            Delimiter.Style = ElementStyle.Compact;

            if (Value.Count() == 0 || Value.Last() == Delimiter.ClosingSpecifier) {
                Delimiter.Style = ElementStyle.Explicit;
            }
        }
        else {
            Delimiter.Style = ElementStyle.Implicit;
        }
    }

    /// <summary>
    /// Serializes this identifier element to its XferLang string representation.
    /// Uses colon delimiters with style determined by identifier validation rules.
    /// </summary>
    /// <param name="formatting">The formatting style to apply during serialization</param>
    /// <param name="indentChar">The character to use for indentation (default: space)</param>
    /// <param name="indentation">The number of indentation characters per level (default: 2)</param>
    /// <param name="depth">The current nesting depth for indentation calculation (default: 0)</param>
    /// <returns>The XferLang string representation of this identifier element</returns>
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        var sb = new StringBuilder();

        if (Delimiter.Style == ElementStyle.Implicit) {
            sb.Append($":{Value}:");
        }
        else if (Delimiter.Style == ElementStyle.Compact) {
            sb.Append($"{Delimiter.Opening}{Value}{Delimiter.Closing}");
        }
        else {
            sb.Append($"{Delimiter.Opening}{Value}{Delimiter.Closing}");
        }

        return sb.ToString();
    }
}
