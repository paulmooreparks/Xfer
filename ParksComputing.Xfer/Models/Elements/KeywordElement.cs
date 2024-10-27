using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;
public class KeywordElement : Element {
    public string Value { get; set; } = string.Empty;
    public TokenType TokenType { get; set; }

    public const char OpeningMarker = '<';
    public const char ClosingMarker = '>';

    public KeywordElement(string text, TokenType tokenType) : base("keyword", new(OpeningMarker, ClosingMarker)) { 
        Value = text;
        TokenType = tokenType;
    }

    public override string ToString() {
        return $"{Value}";
    }
}
