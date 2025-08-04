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
public class KeyValuePairElement : TypedElement<Element>
{
    public static readonly string ElementName = "keyValuePair";

    public TextElement KeyElement { get; set; }
    public string Key { get; }

    public KeyValuePairElement(TextElement keyElement, int specifierCount = 1) : this(keyElement, new EmptyElement(), specifierCount)
    {
    }

    public override Element Value
    {
        get => base.Value;
        set
        {
            // Remove old value from children if it exists
            if (base.Value != null && Children.Contains(base.Value))
            {
                Children.Remove(base.Value);
                base.Value.Parent = null;
            }

            // Set new value
            base.Value = value;

            // Add new value to children
            if (value != null)
            {
                Children.Add(value);
                value.Parent = this;
            }
        }
    }

    public KeyValuePairElement(TextElement keyElement, Element value, int specifierCount = 1)
        : base(value, ElementName, new(specifierCount))
    {
        KeyElement = keyElement;

        if (keyElement is TextElement se)
        {
            Key = se.Value.ToString() ?? string.Empty;
        }
        else if (keyElement is IdentifierElement ke)
        {
            Key = ke.Value.ToString() ?? string.Empty;
        }
        else
        {
            throw new ArgumentException($"Key must be a {nameof(TextElement)} or {nameof(IdentifierElement)} type.");
        }

        // Value is set via base constructor, and Value property setter will handle Children.Add
    }

    public override string ToXfer()
    {
        return ToXfer(Formatting.None);
    }

    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0)
    {
        bool isSpaced = (formatting & Formatting.Spaced) == Formatting.Spaced;
        var sb = new StringBuilder();
        sb.Append(KeyElement.ToXfer(formatting, indentChar, indentation, depth));

        // Add any processing instructions that should appear between key and value
        foreach (var child in Children)
        {
            if (child is ProcessingInstruction pi)
            {
                if (isSpaced)
                {
                    sb.Append(' ');
                }
                sb.Append(pi.ToXfer(formatting, indentChar, indentation, depth));
            }
        }

        if (isSpaced || Value is KeyValuePairElement || Value.Delimiter.Style == ElementStyle.Implicit)
        {
            sb.Append(' ');
        }

        sb.Append(Value?.ToXfer(formatting, indentChar, indentation, depth) ?? new NullElement().ToXfer(formatting, indentChar, indentation, depth));
        return sb.ToString();
    }

    public override string ToString()
    {
        return ToXfer();
    }
}
