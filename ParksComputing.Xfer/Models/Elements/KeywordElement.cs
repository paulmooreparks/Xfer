using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;
public class KeywordElement : Element {
    public static readonly string ElementName = "keyword";
    public const char OpeningMarker = '<';
    public const char ClosingMarker = '>';

    public string Value { get; set; } = string.Empty;

    public KeywordElement(string text) : base(ElementName, new(OpeningMarker, ClosingMarker)) { 
        Value = text;
    }

    public override string ToString() {
        return $"{Value}";
    }
}
