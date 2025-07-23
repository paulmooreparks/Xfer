namespace ParksComputing.Xfer.Lang.Attributes;

/// <summary>
/// Specifies the format for numeric serialization in XferLang.
/// </summary>
public enum XferNumericFormat
{
    /// <summary>
    /// Use default format based on settings and type (implicit for int, compact for others).
    /// </summary>
    Default,

    /// <summary>
    /// Serialize as decimal number: 42
    /// </summary>
    Decimal,

    /// <summary>
    /// Serialize as hexadecimal with prefix: #$2A
    /// </summary>
    Hexadecimal,

    /// <summary>
    /// Serialize as binary with prefix: #%00101010
    /// </summary>
    Binary
}
