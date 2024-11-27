using System.Text.RegularExpressions;

namespace ParksComputing.Xfer.Models.Elements;

public abstract class TextElement : TypedElement<string> {
    public TextElement(string text, string name, ElementDelimiter delimiter) : base(text, name, delimiter) {
    }

    public override string Value {
        get => base.Value;
        set {
            base.Value = value;
            CheckAndUpdateDelimiterStyle();
        }
    }

    protected int GetMaxConsecutiveSpecifiers(string value, char specifier) {
        // Find all sequences of the specifier in the Value
        int maxCount = 0;
        int currentCount = 0;

        foreach (char c in value) {
            if (c == specifier) {
                currentCount++;
            }
            else {
                maxCount = Math.Max(maxCount, currentCount);
                currentCount = 0;
            }
        }

        // Final check in case the last sequence is the longest
        return Math.Max(maxCount, currentCount);
    }

    protected virtual void CheckAndUpdateDelimiterStyle() {
        int maxConsecutiveSpecifiers = GetMaxConsecutiveSpecifiers(Value, Delimiter.ClosingSpecifier);
        Delimiter.SpecifierCount = maxConsecutiveSpecifiers + 1;

        if (Value.Count() == 0 || Value.Last() == Delimiter.ClosingSpecifier) {
            Delimiter.Style = ElementStyle.Explicit;
        }
        else {
            Delimiter.Style = ElementStyle.Compact;
        }
    }

    public override string ToXfer() {
        if (Delimiter.Style == ElementStyle.Implicit) {
            return $"{Value}";
        }
        if (Delimiter.Style == ElementStyle.Compact) {
            return $"{Delimiter.MinOpening}{Value}{Delimiter.MinClosing}";
        }
        return $"{Delimiter.Opening}{Value}{Delimiter.Closing}";
    }

    public override string ToString() {
        return Value;
    }
}
