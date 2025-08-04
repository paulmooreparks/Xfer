using System;

namespace ParksComputing.Xfer.Lang.Attributes;

/// <summary>
/// Specifies the maximum number of decimal places to display when serializing decimal and double values to XferLang.
/// This attribute only affects the string representation and does not modify the underlying value.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class XferDecimalPrecisionAttribute : Attribute
{
    /// <summary>
    /// The maximum number of decimal places to display.
    /// </summary>
    public int DecimalPlaces { get; }

    /// <summary>
    /// Whether to remove trailing zeros after the decimal point.
    /// Default is true.
    /// </summary>
    public bool RemoveTrailingZeros { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the XferDecimalPrecisionAttribute.
    /// </summary>
    /// <param name="decimalPlaces">The maximum number of decimal places to display. Must be 0 or greater.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when decimalPlaces is negative.</exception>
    public XferDecimalPrecisionAttribute(int decimalPlaces)
    {
        if (decimalPlaces < 0) {
            throw new ArgumentOutOfRangeException(nameof(decimalPlaces), "Decimal places must be 0 or greater.");
        }
        
        DecimalPlaces = decimalPlaces;
    }
}
