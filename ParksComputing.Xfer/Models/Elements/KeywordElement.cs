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

    public string TypedValue { get; set; } = string.Empty;
    public override string Value => TypedValue;

    public KeywordElement(string text) : base(ElementName, new(OpeningMarker, ClosingMarker)) { 
        TypedValue = text;
    }

    public override string ToString() {
        return $"{Value}";
    }
}
