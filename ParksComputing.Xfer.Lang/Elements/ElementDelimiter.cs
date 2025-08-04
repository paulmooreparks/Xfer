using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents the delimiter information for an XferLang element, including opening/closing characters,
/// specifier count, and element style. This class manages the construction of element delimiters
/// for different serialization styles.
/// </summary>
public class ElementDelimiter
{
    /// <summary>
    /// Gets the character used to open this element type.
    /// </summary>
    public char OpeningSpecifier { get; }

    /// <summary>
    /// Gets the character used to close this element type.
    /// </summary>
    public char ClosingSpecifier { get; }

    private int _specifierCount;

    /// <summary>
    /// Gets or sets the number of specifier characters to repeat in the delimiter.
    /// Setting this value updates the Opening, Closing, MinOpening, and MinClosing properties.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the style of this element (Explicit, Compact, or Implicit).
    /// </summary>
    public ElementStyle Style { get; set; } = ElementStyle.Explicit;

    /// <summary>
    /// Gets the full opening delimiter string including angle brackets.
    /// </summary>
    public string Opening { get; protected set; }

    /// <summary>
    /// Gets the full closing delimiter string including angle brackets.
    /// </summary>
    public string Closing { get; protected set; }

    /// <summary>
    /// Gets the minimal opening delimiter string without angle brackets.
    /// </summary>
    public string MinOpening { get; protected set; }

    /// <summary>
    /// Gets the minimal closing delimiter string without angle brackets.
    /// </summary>
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
