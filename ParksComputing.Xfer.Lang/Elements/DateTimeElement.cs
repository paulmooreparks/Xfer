using System;
using System.Globalization;
using System.Text;

namespace ParksComputing.Xfer.Lang.Elements
{
    /// <summary>
    /// Represents a date-time element in XferLang using at (@) delimiters.
    /// DateTime elements store DateTime values with support for various formatting
    /// and timezone handling options through the DateTimeHandling property.
    /// </summary>
    public class DateTimeElement : TypedElement<DateTime>
    {
        /// <summary>
        /// The element name used in XferLang serialization for date/time elements.
        /// </summary>
        public static readonly string ElementName = "date";

        /// <summary>
        /// The opening delimiter character (at sign) for date/time elements.
        /// </summary>
        public const char OpeningSpecifier = '@';

        /// <summary>
        /// The closing delimiter character (at sign) for date/time elements.
        /// </summary>
        public const char ClosingSpecifier = OpeningSpecifier;

        /// <summary>
        /// The delimiter configuration for date/time elements using at sign characters.
        /// </summary>
        public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, 1, ElementStyle.Compact);

        /// <summary>
        /// Gets or sets how the DateTime value is formatted during serialization.
        /// Controls UTC, Local, Unspecified, or RoundTrip formatting modes.
        /// </summary>
        public DateTimeHandling DateTimeHandling { get; set; } = DateTimeHandling.RoundTrip;

        /// <summary>
        /// Initializes a new instance of the DateTimeElement class by parsing a DateTime string.
        /// </summary>
        /// <param name="stringValue">The DateTime string to parse (must be in ISO 8601 format)</param>
        /// <param name="dateTimeHandling">How to handle DateTime serialization (default: RoundTrip)</param>
        /// <param name="specifierCount">The number of delimiter characters to use (default: 1)</param>
        /// <param name="elementStyle">The element style for delimiter handling (default: Compact)</param>
        /// <exception cref="InvalidOperationException">Thrown when the string cannot be parsed as a valid DateTime</exception>
        public DateTimeElement(string stringValue, DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
            : this(DateTime.Now, dateTimeHandling, specifierCount, elementStyle)
        {
            if (!DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dateTime))
            {
                throw new InvalidOperationException($"Invalid date string '{stringValue}'. Expected ISO 8601 format.");
            }

            Value = dateTime;
            DateTimeHandling = dateTimeHandling;
        }

        /// <summary>
        /// Initializes a new instance of the DateTimeElement class from a TimeOnly value.
        /// Creates a DateTime with today's date and the specified time.
        /// </summary>
        /// <param name="timeOnly">The TimeOnly value to convert to DateTime</param>
        /// <param name="dateTimeHandling">How to handle DateTime serialization (default: RoundTrip)</param>
        /// <param name="specifierCount">The number of delimiter characters to use (default: 1)</param>
        /// <param name="elementStyle">The element style for delimiter handling (default: Compact)</param>
        public DateTimeElement(TimeOnly timeOnly, DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
            : this(timeOnly.ToLongTimeString(), dateTimeHandling, specifierCount, elementStyle)
        {
            Value = new DateTime(0, 0, 0, timeOnly.Hour, timeOnly.Minute, timeOnly.Second);
        }

        /// <summary>
        /// Initializes a new instance of the DateTimeElement class with the specified DateTime value and formatting options.
        /// </summary>
        /// <param name="dateValue">The DateTime value to represent</param>
        /// <param name="dateTimeHandling">How to handle DateTime serialization (default: RoundTrip)</param>
        /// <param name="specifierCount">The number of delimiter characters to use (default: 1)</param>
        /// <param name="elementStyle">The element style for delimiter handling (default: Compact)</param>
        public DateTimeElement(DateTime dateValue, DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact) : base(dateValue, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier))
        {
            DateTimeHandling = dateTimeHandling;
        }

        /// <summary>
        /// Serializes this DateTime element to its XferLang string representation using default formatting.
        /// </summary>
        /// <returns>The XferLang string representation of this DateTime element</returns>
        public override string ToXfer()
        {
            return ToXfer(Formatting.None);
        }

        /// <summary>
        /// Serializes this DateTime element to its XferLang string representation with specified formatting.
        /// Uses tilde delimiters and applies the configured DateTimeHandling strategy.
        /// </summary>
        /// <param name="formatting">The formatting style to apply during serialization</param>
        /// <param name="indentChar">The character to use for indentation (default: space)</param>
        /// <param name="indentation">The number of indentation characters per level (default: 2)</param>
        /// <param name="depth">The current nesting depth for indentation calculation (default: 0)</param>
        /// <returns>The XferLang string representation of this DateTime element</returns>
        public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0)
        {
            var sb = new StringBuilder();
            sb.Append($"{Delimiter.ExplicitOpening}{ToString()}{Delimiter.ExplicitClosing}");
            return sb.ToString();
        }

        /// <summary>
        /// Returns a string representation of this DateTime element using the configured DateTimeHandling strategy.
        /// </summary>
        /// <returns>The formatted DateTime string according to the DateTimeHandling setting</returns>
        public override string ToString()
        {
            string dateValue = FormatDate(Value, DateTimeHandling);
            return dateValue;
        }

        /// <summary>
        /// Formats a DateTime value according to the specified DateTimeHandling strategy.
        /// Handles UTC, Local, Unspecified, and RoundTrip formatting modes.
        /// </summary>
        /// <param name="dateValue">The DateTime value to format</param>
        /// <param name="dateTimeHandling">The formatting strategy to apply</param>
        /// <returns>The formatted DateTime string</returns>
        public static string FormatDate(DateTime dateValue, DateTimeHandling dateTimeHandling) {
            var formatString = GetFormatString(dateTimeHandling);

            return dateTimeHandling switch {
                DateTimeHandling.Utc => dateValue.ToUniversalTime().ToString(formatString),
                DateTimeHandling.Local => dateValue.ToLocalTime().ToString(formatString),
                DateTimeHandling.Unspecified => dateValue.ToString(formatString),
                _ => dateValue.ToString(formatString), // Round-trip format
            };
        }

        /// <summary>
        /// Gets the appropriate format string for the specified DateTimeHandling strategy.
        /// Returns ISO 8601 formats for different DateTime handling modes.
        /// </summary>
        /// <param name="dateTimeHandling">The DateTimeHandling strategy</param>
        /// <returns>A format string suitable for DateTime.ToString() method</returns>
        public static string GetFormatString(DateTimeHandling dateTimeHandling) {
            return dateTimeHandling switch {
                DateTimeHandling.Utc => "s",
                DateTimeHandling.Local => "s",
                DateTimeHandling.Unspecified => "O",
                _ => "O", // Round-trip format
            };
        }
    }
}
