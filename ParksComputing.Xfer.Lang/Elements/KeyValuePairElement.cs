using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParksComputing.Xfer.Lang.ProcessingInstructions;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a key-value pair element in XferLang, where the key is a text element
/// and the value can be any element type. This is the fundamental building block for
/// object properties and named elements in the XferLang format.
/// </summary>
public class KeyValuePairElement : TypedElement<Element> {
    /// <summary>
    /// The element name used in XferLang serialization for key-value pairs.
    /// </summary>
    public static readonly string ElementName = "keyValuePair";

    /// <summary>
    /// Gets or sets the keyword element that represents the key portion of the key-value pair.
    /// </summary>
    public KeywordElement KeyElement { get; set; }

    /// <summary>
    /// Gets the string representation of the key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Initializes a new instance of the KeyValuePairElement class with a keyword element and default empty value.
    /// </summary>
    /// <param name="keyElement">The keyword element representing the key.</param>
    /// <param name="specifierCount">The number of delimiter characters to use.</param>
    public KeyValuePairElement(KeywordElement keyElement, int specifierCount = 1) : this(keyElement, new EmptyElement(), specifierCount) {
    }

    /// <summary>
    /// Gets or sets the value element of the key-value pair.
    /// </summary>
    public override Element Value {
        get => base.Value;
        set {
            // Remove old value from children if it exists
            if (base.Value != null && Children.Contains(base.Value)) {
                Children.Remove(base.Value);
                base.Value.Parent = null;
            }

            // Set new value
            base.Value = value;

            // Add new value to children
            if (value != null) {
                Children.Add(value);
                value.Parent = this;
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the KeyValuePairElement class with a keyword element and value element.
    /// </summary>
    /// <param name="keyElement">The keyword element representing the key.</param>
    /// <param name="value">The element representing the value.</param>
    /// <param name="specifierCount">The number of delimiter characters to use.</param>
    public KeyValuePairElement(KeywordElement keyElement, Element value, int specifierCount = 1)
        : base(value, ElementName, new(specifierCount)) {
        KeyElement = keyElement;
        Key = keyElement.Value?.ToString() ?? string.Empty;

        // Value is set via base constructor, and Value property setter will handle Children.Add
    }

    /// <summary>
    /// Converts the key-value pair element to its XferLang string representation without formatting.
    /// </summary>
    /// <returns>The XferLang representation of the key-value pair element.</returns>
    public override string ToXfer() {
        return ToXfer(Formatting.None);
    }

    /// <summary>
    /// Converts the key-value pair element to its XferLang string representation with specified formatting options.
    /// </summary>
    /// <param name="formatting">The formatting options to apply.</param>
    /// <param name="indentChar">The character to use for indentation (default is space).</param>
    /// <param name="indentation">The number of indent characters per level (default is 2).</param>
    /// <param name="depth">The current nesting depth (default is 0).</param>
    /// <returns>The formatted XferLang representation of the key-value pair element.</returns>
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        bool isSpaced = (formatting & Formatting.Spaced) == Formatting.Spaced;
        var sb = new StringBuilder();
        sb.Append(KeyElement.ToXfer(formatting, indentChar, indentation, depth));

        // Add any processing instructions that should appear between key and value
        foreach (var child in Children) {
            if (child is ProcessingInstruction pi) {
                if (isSpaced) {
                    sb.Append(' ');
                }
                sb.Append(pi.ToXfer(formatting, indentChar, indentation, depth));
            }
        }

        // Add space between key and value only when needed for disambiguation
        // No space when value has closing delimiters (unambiguous): name"value"
        // Space when value is implicit/no delimiters (ambiguous): name active
        bool needsSpace = false;
        if (Value != null) {
            // Add space if value is implicit (no delimiters) or compact with no closing delimiter
            if (Value.Delimiter.Style == ElementStyle.Implicit) {
                needsSpace = true;
            }
        }

        if (needsSpace) {
            sb.Append(' ');
        }

        sb.Append(Value?.ToXfer(formatting, indentChar, indentation, depth) ?? new NullElement().ToXfer(formatting, indentChar, indentation, depth));
        return sb.ToString();
    }

    /// <summary>
    /// Returns a string representation of the key-value pair element.
    /// </summary>
    /// <returns>The XferLang representation of the key-value pair element.</returns>
    public override string ToString() {
        return ToXfer();
    }
}
