using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;
public class KeywordElement : TextElement {
    public static readonly string ElementName = "keyword";
    public const char OpeningMarker = '<';
    public const char ClosingMarker = '>';

    public KeywordElement(string text) : base(text, ElementName, new(OpeningMarker, ClosingMarker)) { 
    }

    public override string ToString() {
        return $"{Value}";
    }
}
