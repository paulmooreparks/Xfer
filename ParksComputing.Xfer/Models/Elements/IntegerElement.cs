using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class IntegerElement : NumericElement<int> {
    public static readonly string ElementName = "integer";
    public const char OpeningSpecifier = '#';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    public IntegerElement(int value, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Normal)
        : base(value, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, specifierCount, elementStyle)) {
    }
}
