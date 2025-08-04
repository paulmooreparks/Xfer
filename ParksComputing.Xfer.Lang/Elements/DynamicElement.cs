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
    /// <summary>
    /// The element name used in XferLang serialization for dynamic elements.
    /// </summary>
    public static readonly string ElementName = "dynamic";

    /// <summary>
    /// The opening delimiter character (pipe) for dynamic elements.
    /// </summary>
    public const char OpeningSpecifier = '|';

    /// <summary>
    /// The closing delimiter character (pipe) for dynamic elements.
    /// </summary>
    public const char ClosingSpecifier = OpeningSpecifier;

    /// <summary>
    /// The delimiter configuration for dynamic elements using pipe characters.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    /// <summary>
    /// Initializes a new instance of the DynamicElement class with the specified text and formatting options.
    /// </summary>
    /// <param name="text">The dynamic text content that can be resolved at runtime</param>
    /// <param name="specifierCount">The number of delimiter characters to use (default: 1)</param>
    /// <param name="style">The element style for delimiter handling (default: Compact)</param>
    public DynamicElement(string text, int specifierCount = 1, ElementStyle style = ElementStyle.Compact)
        : base(text, ElementName, new(OpeningSpecifier, ClosingSpecifier, specifierCount, style))
    {
    }
}
