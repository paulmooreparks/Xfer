using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a date-only element in XferLang using pipe (|) delimiters.
/// Date elements store DateOnly values and support various formatting options
/// through the DateTimeHandling property. This is used for date values without
/// time components.
/// </summary>
public class DateElement : TypedElement<DateOnly> {
    /// <summary>
    /// The element name used in XferLang serialization for date elements, inherited from DateTimeElement.
    /// </summary>
    public static readonly string ElementName = DateTimeElement.ElementName;

    /// <summary>
    /// The opening delimiter character (tilde) for date elements, inherited from DateTimeElement.
    /// </summary>
    public const char OpeningSpecifier = DateTimeElement.OpeningSpecifier;

    /// <summary>
    /// The closing delimiter character (tilde) for date elements, inherited from DateTimeElement.
    /// </summary>
    public const char ClosingSpecifier = DateTimeElement.ClosingSpecifier;

    /// <summary>
    /// The delimiter configuration for date elements, inherited from DateTimeElement.
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = DateTimeElement.ElementDelimiter;

    /// <summary>
    /// Gets or sets how the date value is formatted during serialization.
    /// Controls UTC, Local, Unspecified, or RoundTrip formatting modes.
    /// </summary>
    public DateTimeHandling DateTimeHandling { get; set; } = DateTimeHandling.RoundTrip;

    /// <summary>
    /// Initializes a new instance of the DateElement class with the current date and specified formatting options.
    /// </summary>
    /// <param name="dateTimeHandling">How to handle date serialization (default: RoundTrip)</param>
    /// <param name="specifierCount">The number of delimiter characters to use (default: 1)</param>
    /// <param name="elementStyle">The element style for delimiter handling (default: Compact)</param>
    public DateElement(DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
        : this(DateOnly.FromDateTime(DateTime.Now), dateTimeHandling, specifierCount, elementStyle) {
    }

    /// <summary>
    /// Initializes a new instance of the DateElement class by parsing a date string.
    /// </summary>
    /// <param name="stringValue">The date string to parse (must be in ISO 8601 format)</param>
    /// <param name="dateTimeHandling">How to handle date serialization (default: RoundTrip)</param>
    /// <param name="specifierCount">The number of delimiter characters to use (default: 1)</param>
    /// <param name="elementStyle">The element style for delimiter handling (default: Compact)</param>
    /// <exception cref="InvalidOperationException">Thrown when the string cannot be parsed as a valid date</exception>
    public DateElement(string stringValue, DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
        : this(DateOnly.FromDateTime(DateTime.Now), dateTimeHandling, specifierCount, elementStyle) {
        if (!DateOnly.TryParse(stringValue, out var dateOnly)) {
            throw new InvalidOperationException($"Invalid date string '{stringValue}'. Expected ISO 8601 format.");
        }

        Value = dateOnly;
        DateTimeHandling = dateTimeHandling;
    }

    /// <summary>
    /// Initializes a new instance of the DateElement class with the specified DateOnly value and formatting options.
    /// </summary>
    /// <param name="dateOnly">The DateOnly value to represent</param>
    /// <param name="dateTimeHandling">How to handle date serialization (default: RoundTrip)</param>
    /// <param name="specifierCount">The number of delimiter characters to use (default: 1)</param>
    /// <param name="elementStyle">The element style for delimiter handling (default: Compact)</param>
    public DateElement(DateOnly dateOnly, DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
        : base(dateOnly, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier)) {
        DateTimeHandling = dateTimeHandling;
    }

    /// <summary>
    /// Serializes this date element to its XferLang string representation using default formatting.
    /// </summary>
    /// <returns>The XferLang string representation of this date element</returns>
    public override string ToXfer() {
        return ToXfer(Formatting.None);
    }

    /// <summary>
    /// Serializes this date element to its XferLang string representation with specified formatting.
    /// Uses tilde delimiters and applies the configured DateTimeHandling strategy.
    /// </summary>
    /// <param name="formatting">The formatting style to apply during serialization</param>
    /// <param name="indentChar">The character to use for indentation (default: space)</param>
    /// <param name="indentation">The number of indentation characters per level (default: 2)</param>
    /// <param name="depth">The current nesting depth for indentation calculation (default: 0)</param>
    /// <returns>The XferLang string representation of this date element</returns>
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        var sb = new StringBuilder();
        sb.Append($"{Delimiter.MinOpening}{ToString()}{Delimiter.MinClosing}");
        return sb.ToString();
    }

    /// <summary>
    /// Returns a string representation of this date element using the configured DateTimeHandling strategy.
    /// </summary>
    /// <returns>The formatted date string according to the DateTimeHandling setting</returns>
    public override string ToString() {
        string dateValue = FormatDate(Value, DateTimeHandling);
        return dateValue;
    }

    /// <summary>
    /// Formats a DateOnly value according to the specified DateTimeHandling strategy.
    /// Handles different formatting modes for date-only values.
    /// </summary>
    /// <param name="dateValue">The DateOnly value to format</param>
    /// <param name="dateTimeHandling">The formatting strategy to apply</param>
    /// <returns>The formatted date string</returns>
    public static string FormatDate(DateOnly dateValue, DateTimeHandling dateTimeHandling) {
        var formatString = GetFormatString(dateTimeHandling);

        return dateTimeHandling switch {
            DateTimeHandling.Utc => dateValue.ToString(formatString),
            DateTimeHandling.Local => dateValue.ToString(formatString),
            DateTimeHandling.Unspecified => dateValue.ToString("O"),
            _ => dateValue.ToString(formatString), // Round-trip format
        };
    }

    /// <summary>
    /// Gets the appropriate format string for the specified DateTimeHandling strategy for date-only values.
    /// Returns ISO 8601 date formats for different handling modes.
    /// </summary>
    /// <param name="dateTimeHandling">The DateTimeHandling strategy</param>
    /// <returns>A format string suitable for DateOnly.ToString() method</returns>
    public static string GetFormatString(DateTimeHandling dateTimeHandling) {
        return dateTimeHandling switch {
            DateTimeHandling.Utc => "O",
            DateTimeHandling.Local => "O",
            DateTimeHandling.Unspecified => "O",
            _ => "O", // Round-trip format
        };
    }
}
