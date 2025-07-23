using System;

namespace ParksComputing.Xfer.Lang.Attributes;

/// <summary>
/// Specifies how a numeric property should be formatted when serialized to XferLang.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class XferNumericFormatAttribute : Attribute
{
    /// <summary>
    /// The format to use for numeric serialization.
    /// </summary>
    public XferNumericFormat Format { get; }

    /// <summary>
    /// For binary format, specifies the minimum number of bits to display.
    /// Default is to use the minimum required bits.
    /// </summary>
    public int MinBits { get; set; } = 0;

    /// <summary>
    /// For hex format, specifies the minimum number of hex digits to display.
    /// Default is to use the minimum required digits.
    /// </summary>
    public int MinDigits { get; set; } = 0;

    /// <summary>
    /// Initializes a new instance of the XferNumericFormatAttribute.
    /// </summary>
    /// <param name="format">The numeric format to use.</param>
    public XferNumericFormatAttribute(XferNumericFormat format)
    {
        Format = format;
    }
}
