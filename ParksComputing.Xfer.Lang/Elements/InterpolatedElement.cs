using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents an interpolated string element in XferLang.
/// Uses single quote (') delimiters and supports string interpolation with embedded expressions.
/// </summary>
public class InterpolatedElement : TextElement
{
    /// <summary>
    /// The element name used in XferLang serialization for interpolated strings.
    /// </summary>
    public static readonly string ElementName = "interpolated";

    /// <summary>
    /// The opening delimiter character (single quote) for interpolated string elements.
    /// </summary>
    public const char OpeningSpecifier = '\'';

    /// <summary>
    /// The closing delimiter character (single quote) for interpolated string elements.
    /// </summary>
    public const char ClosingSpecifier = OpeningSpecifier;

    /// <summary>
    /// The delimiter configuration for interpolated string elements using single quote characters.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    /// <summary>
    /// Initializes a new instance of the InterpolatedElement class with the specified text and formatting options.
    /// </summary>
    /// <param name="text">The interpolated string text with embedded expressions</param>
    /// <param name="specifierCount">The number of delimiter characters to use (default: 1)</param>
    /// <param name="style">The element style for delimiter handling (default: Compact)</param>
    public InterpolatedElement(string text, int specifierCount = 1, ElementStyle style = ElementStyle.Compact)
        : base(text, ElementName, new(OpeningSpecifier, ClosingSpecifier, specifierCount, style))
    {
    }
}
