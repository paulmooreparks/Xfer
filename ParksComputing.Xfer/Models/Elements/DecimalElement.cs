using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class DecimalElement : NumericElement<decimal> {
    public static readonly string ElementName = "decimal";
    public const char OpeningSpecifier = '*';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    public DecimalElement(decimal value, int specifierCount = 1, ElementStyle style = ElementStyle.Minimized) 
        : base(value, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, specifierCount, style)) 
    {
    }
}
