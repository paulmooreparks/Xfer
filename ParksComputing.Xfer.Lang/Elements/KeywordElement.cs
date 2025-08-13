using System.Text;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a keyword element in XferLang using equals (=) delimiters.
/// Keywords are reserved words or language constructs that have special meaning
/// in the XferLang syntax, such as data types, control structures, or built-in functions.
/// </summary>
public class KeywordElement : TextElement
{
    /// <summary>
    /// The element name used for keyword elements.
    /// </summary>
    public static readonly string ElementName = "keyword";

    /// <summary>
    /// The character used to open and close keyword elements ('=').
    /// </summary>
    public const char OpeningSpecifier = '=';

    /// <summary>
    /// The character used to close keyword elements (same as opening).
    /// </summary>
    public const char ClosingSpecifier = OpeningSpecifier;

    /// <summary>
    /// The element delimiter configuration for keyword elements.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    /// <summary>
    /// Initializes a new instance of the KeywordElement class with the specified text and formatting options.
    /// </summary>
    /// <param name="text">The keyword text.</param>
    /// <param name="specifierCount">The number of delimiter characters to use.</param>
    /// <param name="style">The element style to apply (default is implicit).</param>
    public KeywordElement(string text, int specifierCount = 1, ElementStyle style = ElementStyle.Implicit) :
        base(text, ElementName, new(OpeningSpecifier, ClosingSpecifier, specifierCount, style)) {
    }

    /// <summary>
    /// Determines whether a character is valid as the leading character of a keyword.
    /// Keywords must start with a letter or underscore.
    /// </summary>
    /// <param name="c">The character to test.</param>
    /// <returns>True if the character can start a keyword; otherwise, false.</returns>
    public static bool IsKeywordLeadingChar(char c) {
        return char.IsLetter(c);
    }

    /// <summary>
    /// Checks and updates the delimiter style based on the keyword content.
    /// Determines whether the keyword can use implicit style or requires explicit delimiters.
    /// </summary>
    protected override void CheckAndUpdateDelimiterStyle() {
        int maxConsecutiveSpecifiers = GetMaxConsecutiveSpecifiers(Value, Delimiter.ClosingSpecifier);
        Delimiter.SpecifierCount = maxConsecutiveSpecifiers + 1;

        bool isValid = false;

        if (!string.IsNullOrEmpty(Value) && IsKeywordLeadingChar(Value[0])) {
            isValid = true;
            for (int i = 1; i < Value.Length; i++) {
                char c = Value[i];
                if (!(char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.')) {
                    isValid = false;
                    break;
                }
            }
        }

        if (Delimiter.Style == ElementStyle.Implicit && !isValid) {
            Delimiter.Style = ElementStyle.Compact;

            if (Value.Length == 0 || Value.Last() == Delimiter.ClosingSpecifier) {
                Delimiter.Style = ElementStyle.Explicit;
            }
        }
    }

    /// <summary>
    /// Serializes this keyword element to its XferLang string representation.
    /// Keywords are represented with equals (=) delimiters using the keyword name followed by its value.
    /// </summary>
    /// <param name="formatting">The formatting style to apply during serialization</param>
    /// <param name="indentChar">The character to use for indentation (default: space)</param>
    /// <param name="indentation">The number of indentation characters per level (default: 2)</param>
    /// <param name="depth">The current nesting depth for indentation calculation (default: 0)</param>
    /// <returns>The XferLang string representation of this keyword element</returns>
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        var sb = new StringBuilder();

        if (Delimiter.Style == ElementStyle.Implicit) {
            sb.Append($"{Value}");
        }
        else if (Delimiter.Style == ElementStyle.Compact) {
            sb.Append($"{Delimiter.OpeningSpecifier}{Value}");
        }
        else {
            sb.Append($"{Delimiter.ExplicitOpening}{Value}{Delimiter.ExplicitClosing}");
        }

        return sb.ToString();
    }
}
