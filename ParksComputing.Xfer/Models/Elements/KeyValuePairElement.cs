using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;
public class KeyValuePairElement : Element {
    public Element KeyElement { get; set; }
    public string Key { get; }
    public Element Value { get; set; }

    public const char OpeningMarker = '=';
    public const char ClosingMarker = OpeningMarker;

    public KeyValuePairElement(Element key, Element value) : base("keyValuePair", new(OpeningMarker, ClosingMarker)) {
        KeyElement = key;
        Value = value;

        if (key is StringElement se) {
            Key = se.Value;
        } 
        else if (key is KeywordElement ke) {
            Key = ke.Value;
        }
        else {
            throw new ArgumentException("Key must be a StringElement or KeywordElement type.");
        }
    }

    public override string ToString() {
        return $"{KeyElement} {Value}";
    }
}
