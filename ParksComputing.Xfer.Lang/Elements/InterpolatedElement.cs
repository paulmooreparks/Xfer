using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

public class InterpolatedElement : TextElement
{
    public static readonly string ElementName = "interpolated";
    public const char OpeningSpecifier = '\'';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    public InterpolatedElement(string text, int specifierCount = 1, ElementStyle style = ElementStyle.Compact)
        : base(text, ElementName, new(OpeningSpecifier, ClosingSpecifier, specifierCount, style))
    {
    }
}
