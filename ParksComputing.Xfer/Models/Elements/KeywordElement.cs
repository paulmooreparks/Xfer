using System.Text.RegularExpressions;

namespace ParksComputing.Xfer.Models.Elements;
public class KeywordElement : TextElement {
    public static readonly string ElementName = "keyword";
    public const char OpeningSpecifier = ':';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    public KeywordElement(string text, int specifierCount = 1, ElementStyle style = ElementStyle.Implicit) : 
        base(text, ElementName, new(OpeningSpecifier, ClosingSpecifier, specifierCount, style)) 
    {
    }

    public static bool IsKeywordLeadingChar(char c) {
        return char.IsLetter(c) || c == '_';
    }

    protected override void CheckAndUpdateDelimiterStyle() {
        int maxConsecutiveSpecifiers = GetMaxConsecutiveSpecifiers(Value, Delimiter.Closing);
        Delimiter.SpecifierCount = maxConsecutiveSpecifiers + 1;

        if (!Regex.IsMatch(Value, @"^[A-Za-z_][A-Za-z0-9_]*$")) {
            Delimiter.Style = ElementStyle.Compact;

            if (Value.Last() == Delimiter.ClosingSpecifier) {
                Delimiter.Style = ElementStyle.Explicit;
            }
        }
        else {
            Delimiter.Style = ElementStyle.Implicit;
        }
    }
}
