using System.Text;
using System.Text.RegularExpressions;

namespace ParksComputing.Xfer.Elements;

public abstract class TextElement : TypedElement<string>
{
    public TextElement(string text, string name, ElementDelimiter delimiter) : base(text, name, delimiter)
    {
    }

    public override string Value
    {
        get => base.Value;
        set
        {
            base.Value = value;
            CheckAndUpdateDelimiterStyle();
        }
    }

    protected int GetMaxConsecutiveSpecifiers(string value, char specifier)
    {
        // Find all sequences of the specifier in the Value
        int maxCount = 0;
        int currentCount = 0;

        foreach (char c in value)
        {
            if (c == specifier)
            {
                currentCount++;
            }
            else
            {
                maxCount = Math.Max(maxCount, currentCount);
                currentCount = 0;
            }
        }

        // Final check in case the last sequence is the longest
        return Math.Max(maxCount, currentCount);
    }

    protected virtual void CheckAndUpdateDelimiterStyle()
    {
        int maxConsecutiveSpecifiers = GetMaxConsecutiveSpecifiers(Value, Delimiter.ClosingSpecifier);
        Delimiter.SpecifierCount = maxConsecutiveSpecifiers + 1;

        if (Value.Count() == 0 || Value.Last() == Delimiter.ClosingSpecifier)
        {
            Delimiter.Style = ElementStyle.Explicit;
        }
        else
        {
            Delimiter.Style = ElementStyle.Compact;
        }
    }

    public override string ToXfer()
    {
        return ToXfer(Formatting.None);
    }

    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0)
    {
        StringBuilder sb = new();

        if (Delimiter.Style == ElementStyle.Implicit)
        {
            sb.Append(Value);
        }
        else if (Delimiter.Style == ElementStyle.Compact)
        {
            sb.Append($"{Delimiter.MinOpening}{Value}{Delimiter.MinClosing}");
        }
        else
        {
            sb.Append($"{Delimiter.Opening}{Value}{Delimiter.Closing}");
        }

        return sb.ToString();
    }

    public override string ToString()
    {
        return Value;
    }
}
