using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;
public class KeyValuePairElement : Element {
    public string Key { get; set; }
    public Element Value { get; set; }

    public const char OpeningMarker = '=';
    public const char ClosingMarker = OpeningMarker;

    public KeyValuePairElement(string key, Element value) : base("keyValuePair", new(OpeningMarker, ClosingMarker)) { 
        Key = key;
        Value = value;
    }

    public override string ToString() {
        return $"{Key} {Value}";
    }
}
