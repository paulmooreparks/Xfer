using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;
public class KeywordElement : TextElement {
    public static readonly string ElementName = "keyword";
    public const char OpeningSpecifier = ':';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    public KeywordElement(string text, int specifierCount = 1, ElementStyle style = ElementStyle.Normal) : 
        base(text, ElementName, new(OpeningSpecifier, ClosingSpecifier, specifierCount, style)) {
    }
}
