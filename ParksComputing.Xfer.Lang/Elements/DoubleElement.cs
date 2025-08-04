using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a double-precision floating-point number element in XferLang using caret (^) delimiters.
/// Double elements store IEEE 754 double-precision floating-point values suitable for
/// scientific calculations and general-purpose numeric operations.
/// </summary>
public class DoubleElement : NumericElement<double>
{
    public static readonly string ElementName = "double";
    public const char OpeningSpecifier = '^';
    public const char ClosingSpecifier = OpeningSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    public DoubleElement(double value, int markerCount = 1, ElementStyle style = ElementStyle.Compact)
        : base(value, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, markerCount, style))
    {
    }
}
