using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

public class ElementDelimiter
{
    public char OpeningSpecifier { get; }
    public char ClosingSpecifier { get; }

    private int _specifierCount;
    public int SpecifierCount
    {
        get => _specifierCount;
        set
        {
            _specifierCount = value;

            var repeatedOpening = new string(OpeningSpecifier, _specifierCount);
            var repeatedClosing = new string(ClosingSpecifier, _specifierCount);

            Opening = "<" + repeatedOpening;
            Closing = repeatedClosing + ">";
            MinOpening = repeatedOpening;
            MinClosing = repeatedClosing;
        }
    }
    public ElementStyle Style { get; set; } = ElementStyle.Explicit;

    public string Opening { get; protected set; }
    public string Closing { get; protected set; }

    public string MinOpening { get; protected set; }
    public string MinClosing { get; protected set; }

    public ElementDelimiter() : this(default, default, 1) { }

    public ElementDelimiter(int specifierCount) : this(default, default, specifierCount)
    {
    }

    public ElementDelimiter(char openingSpecifier, char closingSpecifier, ElementStyle elementStyle = ElementStyle.Explicit) : this(openingSpecifier, closingSpecifier, 1, elementStyle)
    {
    }

    public ElementDelimiter(char openingSpecifier, char closingSpecifier, int specifierCount, ElementStyle style = ElementStyle.Explicit)
    {
        if (specifierCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(specifierCount), "Count must be at least 1.");
        }

        ValidateSpecifier(openingSpecifier, nameof(openingSpecifier));
        ValidateSpecifier(closingSpecifier, nameof(closingSpecifier));

        Style = style;
        OpeningSpecifier = openingSpecifier;
        ClosingSpecifier = closingSpecifier;
        SpecifierCount = specifierCount;

        var repeatedOpening = new string(openingSpecifier, SpecifierCount);
        var repeatedClosing = new string(closingSpecifier, SpecifierCount);

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
