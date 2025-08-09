using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a time span element in XferLang that stores duration values.
/// Uses the same delimiters as DateTimeElement but focuses specifically on time duration representation
/// with support for various formatting and precision options.
/// </summary>
public class TimeSpanElement : TypedElement<TimeSpan> {
    /// <summary>
    /// The element name used for time span elements (inherited from DateTimeElement).
    /// </summary>
    public static readonly string ElementName = DateTimeElement.ElementName;

    /// <summary>
    /// The character used to open time span elements (inherited from DateTimeElement).
    /// </summary>
    public const char OpeningSpecifier = DateTimeElement.OpeningSpecifier;

    /// <summary>
    /// The character used to close time span elements (inherited from DateTimeElement).
    /// </summary>
    public const char ClosingSpecifier = DateTimeElement.ClosingSpecifier;

    /// <summary>
    /// The element delimiter configuration for time span elements (inherited from DateTimeElement).
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = DateTimeElement.ElementDelimiter;

    /// <summary>
    /// Gets or sets the date/time handling strategy for formatting and parsing time span values.
    /// </summary>
    public DateTimeHandling DateTimeHandling { get; set; } = DateTimeHandling.RoundTrip;

    /// <summary>
    /// Initializes a new instance of the TimeSpanElement class with zero duration and specified formatting options.
    /// </summary>
    /// <param name="dateTimeHandling">The date/time handling strategy to use.</param>
    /// <param name="specifierCount">The number of delimiter characters to use.</param>
    /// <param name="elementStyle">The element style to apply.</param>
    public TimeSpanElement(DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
        : this(TimeSpan.Zero, dateTimeHandling, specifierCount, elementStyle) {
    }

    /// <summary>
    /// Initializes a new instance of the TimeSpanElement class by parsing a duration string with specified formatting options.
    /// </summary>
    /// <param name="stringValue">The string representation of the time span to parse.</param>
    /// <param name="dateTimeHandling">The date/time handling strategy to use.</param>
    /// <param name="specifierCount">The number of delimiter characters to use.</param>
    /// <param name="elementStyle">The element style to apply.</param>
    /// <exception cref="InvalidOperationException">Thrown when the string value cannot be parsed as a valid time span.</exception>
    public TimeSpanElement(string stringValue, DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
        : this(TimeSpan.Zero, dateTimeHandling, specifierCount, elementStyle) {
        if (!TimeSpan.TryParse(stringValue, out var timeSpan)) {
            throw new InvalidOperationException($"Invalid time string '{stringValue}'. Expected ISO 8601 format.");
        }

        Value = timeSpan;
        DateTimeHandling = dateTimeHandling;
    }

    /// <summary>
    /// Initializes a new instance of the TimeSpanElement class with the specified TimeSpan value and formatting options.
    /// </summary>
    /// <param name="timeSpan">The TimeSpan value to wrap in this element.</param>
    /// <param name="dateTimeHandling">The date/time handling strategy to use.</param>
    /// <param name="specifierCount">The number of delimiter characters to use.</param>
    /// <param name="elementStyle">The element style to apply.</param>
    public TimeSpanElement(TimeSpan timeSpan, DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
        : base(timeSpan, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier)) {
        DateTimeHandling = dateTimeHandling;
    }

    /// <summary>
    /// Converts the time span element to its XferLang string representation without formatting.
    /// </summary>
    /// <returns>The XferLang representation of the time span element.</returns>
    public override string ToXfer() {
        return ToXfer(Formatting.None);
    }

    /// <summary>
    /// Converts the time span element to its XferLang string representation with specified formatting options.
    /// </summary>
    /// <param name="formatting">The formatting options to apply.</param>
    /// <param name="indentChar">The character to use for indentation (default is space).</param>
    /// <param name="indentation">The number of indent characters per level (default is 2).</param>
    /// <param name="depth">The current nesting depth (default is 0).</param>
    /// <returns>The formatted XferLang representation of the time span element.</returns>
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        var sb = new StringBuilder();
        sb.Append($"{Delimiter.ExplicitOpening}{ToString()}{Delimiter.ExplicitClosing}");
        return sb.ToString();
    }

    /// <summary>
    /// Returns a string representation of the time span element using the configured DateTimeHandling strategy.
    /// </summary>
    /// <returns>The formatted time span string.</returns>
    public override string ToString() {
        string timeSpanValue = FormatTimeSpan(Value, DateTimeHandling);
        return timeSpanValue;
    }

    /// <summary>
    /// Formats a TimeSpan value according to the specified DateTimeHandling strategy.
    /// </summary>
    /// <param name="timespanValue">The time span value to format.</param>
    /// <param name="dateTimeHandling">The formatting strategy to apply.</param>
    /// <returns>The formatted time span string.</returns>
    public static string FormatTimeSpan(TimeSpan timespanValue, DateTimeHandling dateTimeHandling) {
        // TODO: Do we need this?
        // var formatString = GetFormatString(dateTimeHandling);

        return dateTimeHandling switch {
            DateTimeHandling.Utc => timespanValue.ToString(),
            DateTimeHandling.Local => timespanValue.ToString(),
            DateTimeHandling.Unspecified => timespanValue.ToString(),
            _ => timespanValue.ToString(), // Round-trip format
        };
    }

    /// <summary>
    /// Gets the appropriate format string for the specified DateTimeHandling strategy.
    /// </summary>
    /// <param name="dateTimeHandling">The date/time handling strategy.</param>
    /// <returns>The format string to use for time span formatting.</returns>
    public static string GetFormatString(DateTimeHandling dateTimeHandling) {
        return dateTimeHandling switch {
            DateTimeHandling.Utc => "s",
            DateTimeHandling.Local => "s",
            DateTimeHandling.Unspecified => "s",
            _ => "O", // Round-trip format
        };
    }
}
