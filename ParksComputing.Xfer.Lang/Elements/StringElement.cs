using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a string element in XferLang using double quote (") delimiters.
/// String elements store text values and handle proper escaping of special characters
/// within the XferLang format.
/// </summary>
public class StringElement : TextElement
{
    /// <summary>
    /// The element type name for string elements.
    /// </summary>
    public static readonly string ElementName = "string";

    /// <summary>
    /// The opening delimiter character for string elements.
    /// </summary>
    public const char OpeningSpecifier = '"';

    /// <summary>
    /// The closing delimiter character for string elements.
    /// </summary>
    public const char ClosingSpecifier = OpeningSpecifier;

    /// <summary>
    /// The default element delimiter configuration for string elements.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    /// <summary>
    /// Initializes a new StringElement with an empty string value.
    /// </summary>
    public StringElement() : this(string.Empty) { }

    /// <summary>
    /// Initializes a new StringElement with the specified text value and formatting options.
    /// </summary>
    /// <param name="text">The string value to store.</param>
    /// <param name="specifierCount">The number of delimiter characters to use (default: 1).</param>
    /// <param name="style">The element style for formatting (default: Compact).</param>
    public StringElement(string text, int specifierCount = 1, ElementStyle style = ElementStyle.Compact) :
        base(text, ElementName, new(OpeningSpecifier, ClosingSpecifier, specifierCount, style))
    {
    }
}
