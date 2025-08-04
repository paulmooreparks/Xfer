using ParksComputing.Xfer.Lang.Configuration;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Attributes;
using System;
using System.Linq;
using System.Reflection;

namespace ParksComputing.Xfer.Lang.Helpers {
    /// <summary>
    /// Helper class for determining safe and appropriate element styles.
    /// </summary>
    internal static class ElementStyleHelper {
        /// <summary>
        /// Determines the appropriate ElementStyle for a string based on content and settings.
        /// </summary>
        public static ElementStyle GetStringStyle(string value, ElementStylePreference preference) {
            return preference switch {
                ElementStylePreference.Explicit => ElementStyle.Explicit,
                ElementStylePreference.ForceCompact => ElementStyle.Compact,
                ElementStylePreference.CompactWhenSafe or ElementStylePreference.MinimalWhenSafe =>
                    IsStringSafeForCompact(value) ? ElementStyle.Compact : ElementStyle.Explicit,
                _ => ElementStyle.Explicit
            };
        }

        /// <summary>
        /// Determines the appropriate ElementStyle for integers based on settings.
        /// Decimal integers are inherently safe and default to implicit style.
        /// </summary>
        public static ElementStyle GetIntegerStyle(int value, ElementStylePreference preference, bool preferImplicit) {
            return preference switch {
                ElementStylePreference.Explicit => ElementStyle.Explicit, // Maximum safety when requested
                ElementStylePreference.ForceCompact => ElementStyle.Compact, // Force compact when requested
                ElementStylePreference.CompactWhenSafe => ElementStyle.Implicit, // Decimal integers are always safe
                ElementStylePreference.MinimalWhenSafe => ElementStyle.Implicit, // Use most minimal for safe integers
                _ => ElementStyle.Implicit // Default: decimal integers are inherently safe
            };
        }

        /// <summary>
        /// Determines the appropriate ElementStyle for long integers based on settings.
        /// Longs default to compact to preserve type information (distinguish from int).
        /// </summary>
        public static ElementStyle GetLongStyle(long value, ElementStylePreference preference) {
            return preference switch {
                ElementStylePreference.Explicit => ElementStyle.Explicit, // Maximum safety when requested
                ElementStylePreference.ForceCompact => ElementStyle.Compact, // Force compact when requested
                _ => ElementStyle.Compact // Default: compact to preserve type information
            };
        }

        /// <summary>
        /// Determines the appropriate ElementStyle for decimal values based on settings.
        /// Decimals default to compact to preserve type information (distinguish from int/double).
        /// </summary>
        public static ElementStyle GetDecimalStyle(decimal value, ElementStylePreference preference) {
            return preference switch {
                ElementStylePreference.Explicit => ElementStyle.Explicit, // Maximum safety when requested
                ElementStylePreference.ForceCompact => ElementStyle.Compact, // Force compact when requested
                _ => ElementStyle.Compact // Default: compact to preserve type information
            };
        }

        /// <summary>
        /// Determines the appropriate ElementStyle for double values based on settings.
        /// Doubles default to compact to preserve type information (distinguish from int/decimal).
        /// </summary>
        public static ElementStyle GetDoubleStyle(double value, ElementStylePreference preference) {
            return preference switch {
                ElementStylePreference.Explicit => ElementStyle.Explicit, // Maximum safety when requested
                ElementStylePreference.ForceCompact => ElementStyle.Compact, // Force compact when requested
                _ => ElementStyle.Compact // Default: compact to preserve type information
            };
        }

        /// <summary>
        /// Creates a formatted integer element based on property attributes.
        /// </summary>
        public static IntegerElement CreateFormattedIntegerElement(int value, PropertyInfo? property, ElementStylePreference preference, bool preferImplicit) {
            var formatAttribute = property?.GetCustomAttribute<XferNumericFormatAttribute>();
            var style = GetIntegerStyle(value, preference, preferImplicit);

            if (formatAttribute != null && formatAttribute.Format != XferNumericFormat.Default && formatAttribute.Format != XferNumericFormat.Decimal) {
                var formatter = new Func<int, string>(v => NumericFormatter.FormatInteger(v, formatAttribute.Format, formatAttribute.MinBits, formatAttribute.MinDigits));
                return new IntegerElement(value, elementStyle: style, customFormatter: formatter);
            }

            return new IntegerElement(value, elementStyle: style);
        }

        /// <summary>
        /// Creates a formatted long element based on property attributes.
        /// </summary>
        public static LongElement CreateFormattedLongElement(long value, PropertyInfo? property, ElementStylePreference preference) {
            var formatAttribute = property?.GetCustomAttribute<XferNumericFormatAttribute>();
            var style = GetLongStyle(value, preference);

            if (formatAttribute != null && formatAttribute.Format != XferNumericFormat.Default && formatAttribute.Format != XferNumericFormat.Decimal) {
                var formatter = new Func<long, string>(v => NumericFormatter.FormatLong(v, formatAttribute.Format, formatAttribute.MinBits, formatAttribute.MinDigits));
                return new LongElement(value, style: style, customFormatter: formatter);
            }

            return new LongElement(value, style: style);
        }

        /// <summary>
        /// Creates a formatted decimal element based on property attributes.
        /// </summary>
        public static DecimalElement CreateFormattedDecimalElement(decimal value, PropertyInfo? property, ElementStylePreference preference) {
            var formatAttribute = property?.GetCustomAttribute<XferNumericFormatAttribute>();
            var precisionAttribute = property?.GetCustomAttribute<XferDecimalPrecisionAttribute>();
            var style = GetDecimalStyle(value, preference);

            // Handle precision formatting
            if (precisionAttribute != null || (formatAttribute != null && formatAttribute.Format == XferNumericFormat.Decimal)) {
                var formatter = new Func<decimal, string>(v => {
                    var format = formatAttribute?.Format ?? XferNumericFormat.Decimal;
                    var minBits = formatAttribute?.MinBits ?? 0;
                    var minDigits = formatAttribute?.MinDigits ?? 0;
                    var decimalPlaces = precisionAttribute?.DecimalPlaces;
                    var removeTrailingZeros = precisionAttribute?.RemoveTrailingZeros ?? true;
                    
                    return NumericFormatter.FormatDecimal(v, format, minBits, minDigits, decimalPlaces, removeTrailingZeros);
                });
                return new DecimalElement(value, style: style, customFormatter: formatter);
            }
            
            // Handle hex/binary formatting (loses fractional precision)
            if (formatAttribute != null && formatAttribute.Format != XferNumericFormat.Default && formatAttribute.Format != XferNumericFormat.Decimal) {
                var formatter = new Func<decimal, string>(v => NumericFormatter.FormatDecimal(v, formatAttribute.Format, formatAttribute.MinBits, formatAttribute.MinDigits));
                return new DecimalElement(value, style: style, customFormatter: formatter);
            }

            return new DecimalElement(value, style: style);
        }

        /// <summary>
        /// Creates a formatted double element based on property attributes.
        /// </summary>
        public static DoubleElement CreateFormattedDoubleElement(double value, PropertyInfo? property, ElementStylePreference preference) {
            var formatAttribute = property?.GetCustomAttribute<XferNumericFormatAttribute>();
            var precisionAttribute = property?.GetCustomAttribute<XferDecimalPrecisionAttribute>();
            var style = GetDoubleStyle(value, preference);

            // Handle precision formatting
            if (precisionAttribute != null || (formatAttribute != null && formatAttribute.Format == XferNumericFormat.Decimal)) {
                var formatter = new Func<double, string>(v => {
                    var format = formatAttribute?.Format ?? XferNumericFormat.Decimal;
                    var minBits = formatAttribute?.MinBits ?? 0;
                    var minDigits = formatAttribute?.MinDigits ?? 0;
                    var decimalPlaces = precisionAttribute?.DecimalPlaces;
                    var removeTrailingZeros = precisionAttribute?.RemoveTrailingZeros ?? true;
                    
                    return NumericFormatter.FormatDouble(v, format, minBits, minDigits, decimalPlaces, removeTrailingZeros);
                });
                return new DoubleElement(value, style: style, customFormatter: formatter);
            }
            
            // Handle hex/binary formatting (loses fractional precision)
            if (formatAttribute != null && formatAttribute.Format != XferNumericFormat.Default && formatAttribute.Format != XferNumericFormat.Decimal) {
                var formatter = new Func<double, string>(v => NumericFormatter.FormatDouble(v, formatAttribute.Format, formatAttribute.MinBits, formatAttribute.MinDigits));
                return new DoubleElement(value, style: style, customFormatter: formatter);
            }

            return new DoubleElement(value, style: style);
        }

        /// <summary>
        /// Checks if a string is safe to serialize using compact syntax.
        /// Safe strings don't contain quotes or other problematic characters.
        /// </summary>
        private static bool IsStringSafeForCompact(string value) {
            if (string.IsNullOrEmpty(value)) {
                return true;
            }

            // Check for characters that would require escaping or cause parsing issues
            return !value.Contains('"') &&
                   !value.Contains('<') &&
                   !value.Contains('>') &&
                   !value.Contains('\n') &&
                   !value.Contains('\r') &&
                   !value.Contains('\t') &&
                   !StartsWithSpecialCharacter(value) &&
                   !ContainsWhitespace(value);
        }

        /// <summary>
        /// Checks if an integer is safe for implicit syntax (no prefix).
        /// Typically safe for simple positive integers that won't be confused with other syntax.
        /// </summary>
        private static bool IsIntegerSafeForImplicit(int value) {
            // Simple positive integers are generally safe for implicit syntax
            // Avoid edge cases like very large numbers or specific patterns
            return value >= 0 && value <= 999999;
        }

        /// <summary>
        /// Checks if string starts with characters that have special meaning in XferLang.
        /// </summary>
        private static bool StartsWithSpecialCharacter(string value) {
            if (string.IsNullOrEmpty(value)) {
                return false;
            }

            char first = value[0];
            return first == '#' || first == '*' || first == '~' ||
                   first == '?' || first == '@' || first == '\\' ||
                   first == '\'' || first == '!' || first == '/';
        }

        /// <summary>
        /// Checks if string contains whitespace that might cause parsing issues.
        /// </summary>
        private static bool ContainsWhitespace(string value) {
            return value.Any(char.IsWhiteSpace);
        }
    }
}
