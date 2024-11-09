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
    public Element TypedValue { get; set; }
    public override string Value => TypedValue.ToString();

    public KeyValuePairElement(Element keyElement, int markerCount = 1) : base(ElementName, new(OpeningMarker, ClosingMarker, markerCount)) {
        KeyElement = keyElement;
        TypedValue = new EmptyElement();

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

    public KeyValuePairElement(Element keyElement, Element value, int markerCount = 1) : this(keyElement, markerCount) {
        TypedValue = value;
    }

    public override string ToString() {
        return $"{Delimiter.Opening}{KeyElement}{TypedValue}{Delimiter.Closing}";
    }
}
