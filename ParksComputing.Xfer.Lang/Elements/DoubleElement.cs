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
    /// <summary>
    /// The element name used in XferLang serialization for double-precision numbers.
    /// </summary>
    public static readonly string ElementName = "double";

    /// <summary>
    /// The opening delimiter character (caret) for double elements.
    /// </summary>
    public const char OpeningSpecifier = '^';

    /// <summary>
    /// The closing delimiter character (caret) for double elements.
    /// </summary>
    public const char ClosingSpecifier = OpeningSpecifier;

    /// <summary>
    /// The delimiter configuration for double elements using caret characters.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

    /// <summary>
    /// Initializes a new instance of the DoubleElement class with the specified value and formatting options.
    /// </summary>
    /// <param name="value">The double-precision floating-point value to represent</param>
    /// <param name="markerCount">The number of delimiter characters to use (default: 1)</param>
    /// <param name="style">The element style for delimiter handling (default: Compact)</param>
    public DoubleElement(double value, int markerCount = 1, ElementStyle style = ElementStyle.Compact)
        : base(value, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, markerCount, style))
    {
    }
}
