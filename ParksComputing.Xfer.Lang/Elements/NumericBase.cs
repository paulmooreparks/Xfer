namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents the numeric base used when interpreting or serializing a <see cref="NumericValue{T}"/>.
/// Prefix conventions (when serialized through helper types):
/// <list type="bullet">
/// <item><description><see cref="Decimal"/>: no prefix</description></item>
/// <item><description><see cref="Hexadecimal"/>: '$' prefix</description></item>
/// <item><description><see cref="Binary"/>: '%' prefix</description></item>
/// </list>
/// </summary>
public enum NumericBase {
    /// <summary>Base 10 (no prefix)</summary>
    Decimal = 10,
    /// <summary>Base 16 ('$' prefix)</summary>
    Hexadecimal = 16,
    /// <summary>Base 2 ('%' prefix)</summary>
    Binary = 2
}
