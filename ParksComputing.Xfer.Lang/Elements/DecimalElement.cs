using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a decimal number element in XferLang using asterisk (*) delimiters.
/// Decimal elements store high-precision decimal values suitable for financial
/// calculations and other applications requiring exact decimal representation.
/// </summary>
public class DecimalElement : NumericElement<decimal>
{
    /// <summary>
    /// The element name used in XferLang serialization for decimal numbers.
    /// </summary>
    public static readonly string ElementName = "decimal";

    /// <summary>
    /// The opening delimiter character (asterisk) for decimal elements.
    /// </summary>
    public const char OpeningSpecifier = '*';

    /// <summary>
    /// The closing delimiter character (asterisk) for decimal elements.
    /// </summary>
    public const char ClosingSpecifier = OpeningSpecifier;

    /// <summary>
    /// The delimiter configuration for decimal elements using asterisk characters.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    /// <summary>
    /// Initializes a new instance of the DecimalElement class with the specified value and formatting options.
    /// </summary>
    /// <param name="value">The decimal value to represent</param>
    /// <param name="specifierCount">The number of delimiter characters to use (default: 1)</param>
    /// <param name="style">The element style for delimiter handling (default: Compact)</param>
    public DecimalElement(decimal value, int specifierCount = 1, ElementStyle style = ElementStyle.Compact)
        : base(value, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, specifierCount, style))
    {
    }
}
