using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class ElementDelimiter
{
    public char OpeningSpecifier { get; }
    public char ClosingSpecifier { get; }
    public int SpecifierCount { get; set; }
    public ElementStyle Style { get; } = ElementStyle.Normal;

    public string Opening { get; }
    public string Closing { get; }

    public string MinOpening { get; }
    public string MinClosing { get; } 

    public ElementDelimiter() : this(default, default, 1) { }

    public ElementDelimiter(int specifierCount) : this(default, default, specifierCount)
    {
    }

    public ElementDelimiter(char openingSpecifier, char closingSpecifier, ElementStyle elementStyle = ElementStyle.Normal) : this(openingSpecifier, closingSpecifier, 1, elementStyle)
    {
    }

    public ElementDelimiter(char openingSpecifier, char closingSpecifier, int specifierCount, ElementStyle style = ElementStyle.Normal)
    {
        if (specifierCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(specifierCount), "Count must be at least 1.");
        }

        ValidateSpecifier(openingSpecifier, nameof(openingSpecifier));
        ValidateSpecifier(closingSpecifier, nameof(closingSpecifier));

        OpeningSpecifier = openingSpecifier;
        ClosingSpecifier = closingSpecifier;
        SpecifierCount = specifierCount;
        Style = style;

        var repeatedOpening = new string(openingSpecifier, specifierCount);
        var repeatedClosing = new string(closingSpecifier, specifierCount);

        Opening = "<" + repeatedOpening;
        Closing = repeatedClosing + ">";
        MinOpening = repeatedOpening;
        MinClosing = repeatedClosing;
    }

    private static void ValidateSpecifier(char specifier, string paramName)
    {
        if (char.IsWhiteSpace(specifier) || char.IsLetterOrDigit(specifier))
        {
            throw new ArgumentException("Specifier cannot be an alphanumeric or whitespace character.", paramName);
        }
    }

    public override string ToString()
    {
        return $"{Opening}...{Closing}";
    }
}
