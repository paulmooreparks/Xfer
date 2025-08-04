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
    public static readonly string ElementName = "identifier";
    public const char OpeningSpecifier = ':';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    public IdentifierElement(string text, int specifierCount = 1, ElementStyle style = ElementStyle.Implicit) :
        base(text, ElementName, new(OpeningSpecifier, ClosingSpecifier, specifierCount, style)) {
    }

    public static bool IsIdentifierLeadingChar(char c) {
        return char.IsLetter(c) || c == '_';
    }

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

    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        var sb = new StringBuilder();

        if (Delimiter.Style == ElementStyle.Implicit) {
            sb.Append($"{Value}");
        }
        else if (Delimiter.Style == ElementStyle.Compact) {
            sb.Append($"{Delimiter.OpeningSpecifier}{Value}");
        }
        else {
            sb.Append($"{Delimiter.Opening}{Value}{Delimiter.Closing}");
        }

        return sb.ToString();
    }
}
