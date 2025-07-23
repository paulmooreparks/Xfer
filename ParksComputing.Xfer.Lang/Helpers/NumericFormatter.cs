using ParksComputing.Xfer.Lang.Attributes;
using System;

namespace ParksComputing.Xfer.Lang.Helpers;

/// <summary>
/// Helper class for formatting numeric values according to XferLang specifications.
/// </summary>
internal static class NumericFormatter
{
    /// <summary>
    /// Formats an integer according to the specified format.
    /// </summary>
    public static string FormatInteger(int value, XferNumericFormat format, int minBits = 0, int minDigits = 0)
    {
        return format switch
        {
            XferNumericFormat.Decimal => value.ToString(),
            XferNumericFormat.Hexadecimal => FormatHexadecimal(value, minDigits),
            XferNumericFormat.Binary => FormatBinary(value, minBits),
            _ => value.ToString()
        };
    }

    /// <summary>
    /// Formats a long integer according to the specified format.
    /// </summary>
    public static string FormatLong(long value, XferNumericFormat format, int minBits = 0, int minDigits = 0)
    {
        return format switch
        {
            XferNumericFormat.Decimal => value.ToString(),
            XferNumericFormat.Hexadecimal => FormatHexadecimal(value, minDigits),
            XferNumericFormat.Binary => FormatBinary(value, minBits),
            _ => value.ToString()
        };
    }

    /// <summary>
    /// Formats a decimal according to the specified format.
    /// Note: Hex and binary formats convert to long, losing fractional precision.
    /// </summary>
    public static string FormatDecimal(decimal value, XferNumericFormat format, int minBits = 0, int minDigits = 0)
    {
        return format switch
        {
            XferNumericFormat.Decimal => value.ToString(),
            XferNumericFormat.Hexadecimal => FormatHexadecimal((long)value, minDigits),
            XferNumericFormat.Binary => FormatBinary((long)value, minBits),
            _ => value.ToString()
        };
    }

    /// <summary>
    /// Formats a double according to the specified format.
    /// Note: Hex and binary formats convert to long, losing fractional precision.
    /// </summary>
    public static string FormatDouble(double value, XferNumericFormat format, int minBits = 0, int minDigits = 0)
    {
        return format switch
        {
            XferNumericFormat.Decimal => value.ToString(),
            XferNumericFormat.Hexadecimal => FormatHexadecimal((long)value, minDigits),
            XferNumericFormat.Binary => FormatBinary((long)value, minBits),
            _ => value.ToString()
        };
    }

    /// <summary>
    /// Formats a value as hexadecimal with XferLang prefix: #$
    /// </summary>
    private static string FormatHexadecimal(long value, int minDigits)
    {
        string hex = Math.Abs(value).ToString("X");
        if (minDigits > 0 && hex.Length < minDigits)
        {
            hex = hex.PadLeft(minDigits, '0');
        }

        string prefix = value < 0 ? "-#$" : "#$";
        return prefix + hex;
    }

    /// <summary>
    /// Formats a value as binary with XferLang prefix: #%
    /// </summary>
    private static string FormatBinary(long value, int minBits)
    {
        string binary = Convert.ToString(Math.Abs(value), 2);
        if (minBits > 0 && binary.Length < minBits)
        {
            binary = binary.PadLeft(minBits, '0');
        }

        string prefix = value < 0 ? "-#%" : "#%";
        return prefix + binary;
    }
}
