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
    public static readonly string ElementName = "decimal";
    public const char OpeningSpecifier = '*';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    public DecimalElement(decimal value, int specifierCount = 1, ElementStyle style = ElementStyle.Compact)
        : base(value, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, specifierCount, style))
    {
    }
}
