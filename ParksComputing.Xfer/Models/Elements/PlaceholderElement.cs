using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class PlaceholderElement : TextElement {
    public static readonly string ElementName = "placeholder";
    public const char OpeningSpecifier = '|';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    public PlaceholderElement(string text, int specifierCount = 1, ElementStyle style = ElementStyle.Minimized) 
        : base(text, ElementName, new(OpeningSpecifier, ClosingSpecifier, specifierCount, style)) 
    {
    }
}
