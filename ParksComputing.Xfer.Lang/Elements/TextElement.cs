using System.Text;
using System.Text.RegularExpressions;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Abstract base class for text-based elements in XferLang that store string values.
/// Provides functionality for handling text delimiters and automatic delimiter count adjustment
/// based on content to prevent parsing conflicts.
/// </summary>
public abstract class TextElement : TypedElement<string> {
    /// <summary>
    /// Initializes a new TextElement with the specified text, name, and delimiter configuration.
    /// </summary>
    /// <param name="text">The text value to store.</param>
    /// <param name="name">The element type name.</param>
    /// <param name="delimiter">The delimiter configuration for this element.</param>
    public TextElement(string text, string name, ElementDelimiter delimiter) : base(text, name, delimiter) {
    }

    /// <summary>
    /// Gets or sets the text value of this element.
    /// Setting the value automatically adjusts delimiter count if needed to prevent parsing conflicts.
    /// </summary>
    public override string Value {
        get => base.Value;
        set {
            base.Value = value;
            CheckAndUpdateDelimiterStyle();
        }
    }

    /// <summary>
    /// Determines the maximum consecutive occurrences of a specific character in the text value.
    /// Used to calculate appropriate delimiter counts to avoid parsing conflicts.
    /// </summary>
    /// <param name="value">The text to analyze.</param>
    /// <param name="specifier">The character to count consecutive occurrences of.</param>
    /// <returns>The maximum number of consecutive occurrences of the character.</returns>
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

    /// <summary>
    /// Checks the current text value and updates the delimiter style and count as needed
    /// to prevent parsing conflicts. Called automatically when the Value property is set.
    /// </summary>
    protected virtual void CheckAndUpdateDelimiterStyle() {
        if (Delimiter.Style != ElementStyle.Explicit) {
            int maxConsecutiveSpecifiers = GetMaxConsecutiveSpecifiers(Value, Delimiter.ClosingSpecifier);
            Delimiter.SpecifierCount = maxConsecutiveSpecifiers + 1;

            if (Value.Count() == 0 || Value.Last() == Delimiter.ClosingSpecifier) {
                Delimiter.Style = ElementStyle.Explicit;
            }
            else {
                Delimiter.Style = ElementStyle.Compact;
            }
        }
    }

    /// <summary>
    /// Converts the text element to its XferLang string representation without formatting.
    /// </summary>
    /// <returns>The XferLang string representation of the text value.</returns>
    public override string ToXfer() {
        return ToXfer(Formatting.None);
    }

    /// <summary>
    /// Converts the text element to its XferLang string representation with formatting options.
    /// </summary>
    /// <param name="formatting">The formatting style to apply.</param>
    /// <param name="indentChar">The character to use for indentation.</param>
    /// <param name="indentation">The number of indent characters per level.</param>
    /// <param name="depth">The current indentation depth.</param>
    /// <returns>The formatted XferLang string representation of the text value.</returns>
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        StringBuilder sb = new();

        if (Delimiter.Style == ElementStyle.Implicit) {
            sb.Append(Value);
        }
        else if (Delimiter.Style == ElementStyle.Compact) {
            sb.Append($"{Delimiter.CompactOpening}{Value}{Delimiter.CompactClosing}");
        }
        else {
            sb.Append($"{Delimiter.ExplicitOpening}{Value}{Delimiter.ExplicitClosing}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns the text value of this element.
    /// </summary>
    /// <returns>The text value as a string.</returns>
    public override string ToString() {
        return Value;
    }
}
