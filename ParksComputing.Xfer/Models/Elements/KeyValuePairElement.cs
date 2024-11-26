using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;
public class KeyValuePairElement : TypedElement<Element> {
    public static readonly string ElementName = "keyValuePair";

    public TextElement KeyElement { get; set; }
    public string Key { get; }

    public KeyValuePairElement(TextElement keyElement, int specifierCount = 1) : this(keyElement, new EmptyElement(), specifierCount) {
    }

    public KeyValuePairElement(TextElement keyElement, Element value, int specifierCount = 1) 
        : base(value, ElementName, new(specifierCount)) 
    {
        KeyElement = keyElement;

        if (keyElement is TextElement se) {
            Key = se.Value.ToString() ?? string.Empty;
        }
        else if (keyElement is KeywordElement ke) {
            Key = ke.Value.ToString() ?? string.Empty;
        }
        else {
            throw new ArgumentException($"Key must be a {nameof(TextElement)} or {nameof(KeywordElement)} type.");
        }
    }

    public override string ToXfer() {
        var sb = new StringBuilder();
        sb.Append(KeyElement.ToXfer());
        if (Value is KeyValuePairElement || Value.Delimiter.Style == ElementStyle.Implicit) {
            sb.Append(' ');
        }
        sb.Append(Value.ToXfer());
        return sb.ToString();
    }

    public override string ToString() {
        return ToXfer();
    }
}
