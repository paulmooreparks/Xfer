using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a dynamic element in XferLang using pipe (|) delimiters.
/// Dynamic elements contain text that can be dynamically resolved or evaluated
/// at runtime, often used for variable substitution or dynamic content generation.
/// </summary>
public class DynamicElement : TextElement
{
    public static readonly string ElementName = "dynamic";
    public const char OpeningSpecifier = '|';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    public DynamicElement(string text, int specifierCount = 1, ElementStyle style = ElementStyle.Compact)
        : base(text, ElementName, new(OpeningSpecifier, ClosingSpecifier, specifierCount, style))
    {
    }
}
