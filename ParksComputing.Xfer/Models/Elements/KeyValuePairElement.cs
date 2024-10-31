using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;
public class KeyValuePairElement : Element {
    public static readonly string ElementName = "keyValuePair";
    public const char OpeningMarker = ':';
    public const char ClosingMarker = OpeningMarker;

    public Element KeyElement { get; set; }
    public string Key { get; }
    public Element Value { get; set; }

    public KeyValuePairElement(Element keyElement) : base(ElementName, new(OpeningMarker, ClosingMarker)) {
        KeyElement = keyElement;
        Value = new EmptyElement();

        if (keyElement is StringElement se) {
            Key = se.Value;
        }
        else if (keyElement is KeywordElement ke) {
            Key = ke.Value;
        }
        else {
            throw new ArgumentException("Key must be a StringElement or KeywordElement type.");
        }
    }

    public KeyValuePairElement(Element keyElement, Element value) : this(keyElement) {
        Value = value;
    }

    public override string ToString() {
        return $"{Delimiter.Opening}{KeyElement} {Value}{Delimiter.Closing}";
    }
}
