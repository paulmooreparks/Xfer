using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

/// <summary>
/// Represents a time-only element in XferLang that stores time values without date components.
/// Uses the same delimiters as DateTimeElement but focuses specifically on time representation
/// with support for various time formatting and precision options.
/// </summary>
public class TimeElement : TypedElement<TimeOnly> {
    /// <summary>
    /// The element name used for time elements (inherited from DateTimeElement).
    /// </summary>
    public static readonly string ElementName = DateTimeElement.ElementName;

    /// <summary>
    /// The character used to open time elements (inherited from DateTimeElement).
    /// </summary>
    public const char OpeningSpecifier = DateTimeElement.OpeningSpecifier;

    /// <summary>
    /// The character used to close time elements (inherited from DateTimeElement).
    /// </summary>
    public const char ClosingSpecifier = DateTimeElement.ClosingSpecifier;

    /// <summary>
    /// The element delimiter configuration for time elements (inherited from DateTimeElement).
    /// </summary>
    public static readonly ElementDelimiter ElementDelimiter = DateTimeElement.ElementDelimiter;

    /// <summary>
    /// Gets or sets the date/time handling strategy for formatting and parsing time values.
    /// </summary>
    public DateTimeHandling DateTimeHandling { get; set; } = DateTimeHandling.RoundTrip;

    /// <summary>
    /// Initializes a new instance of the TimeElement class with the current time and specified formatting options.
    /// </summary>
    /// <param name="dateTimeHandling">The date/time handling strategy to use.</param>
    /// <param name="specifierCount">The number of delimiter characters to use.</param>
    /// <param name="elementStyle">The element style to apply.</param>
    public TimeElement(DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
        : this(TimeOnly.FromDateTime(DateTime.Now), dateTimeHandling, specifierCount, elementStyle)
    {
    }

    /// <summary>
    /// Initializes a new instance of the TimeElement class by parsing a time string with specified formatting options.
    /// </summary>
    /// <param name="stringValue">The string representation of the time to parse.</param>
    /// <param name="dateTimeHandling">The date/time handling strategy to use.</param>
    /// <param name="specifierCount">The number of delimiter characters to use.</param>
    /// <param name="elementStyle">The element style to apply.</param>
    /// <exception cref="InvalidOperationException">Thrown when the string value cannot be parsed as a valid time.</exception>
    public TimeElement(string stringValue, DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
        : this(TimeOnly.FromDateTime(DateTime.Now), dateTimeHandling, specifierCount, elementStyle)
    {
        if (!TimeOnly.TryParse(stringValue, out var timeOnly)) {
            throw new InvalidOperationException($"Invalid time string '{stringValue}'. Expected ISO 8601 format.");
        }

        Value = timeOnly;
        DateTimeHandling = dateTimeHandling;
    }

    /// <summary>
    /// Initializes a new instance of the TimeElement class with the specified TimeOnly value and formatting options.
    /// </summary>
    /// <param name="timeOnly">The TimeOnly value to wrap in this element.</param>
    /// <param name="dateTimeHandling">The date/time handling strategy to use.</param>
    /// <param name="specifierCount">The number of delimiter characters to use.</param>
    /// <param name="elementStyle">The element style to apply.</param>
    public TimeElement(TimeOnly timeOnly, DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
        : base(timeOnly, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier))
    {
        DateTimeHandling = dateTimeHandling;
    }

    /// <summary>
    /// Converts the time element to its XferLang string representation without formatting.
    /// </summary>
    /// <returns>The XferLang representation of the time element.</returns>
    public override string ToXfer() {
        return ToXfer(Formatting.None);
    }

    /// <summary>
    /// Converts the time element to its XferLang string representation with specified formatting options.
    /// </summary>
    /// <param name="formatting">The formatting options to apply.</param>
    /// <param name="indentChar">The character to use for indentation (default is space).</param>
    /// <param name="indentation">The number of indent characters per level (default is 2).</param>
    /// <param name="depth">The current nesting depth (default is 0).</param>
    /// <returns>The formatted XferLang representation of the time element.</returns>
    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        var sb = new StringBuilder();
        sb.Append($"{Delimiter.MinOpening}{ToString()}{Delimiter.MinClosing}");
        return sb.ToString();
    }

    /// <summary>
    /// Returns a string representation of the time element using the configured DateTimeHandling strategy.
    /// </summary>
    /// <returns>The formatted time string.</returns>
    public override string ToString() {
        string timeValue = FormatTime(Value, DateTimeHandling);
        return timeValue;
    }

    /// <summary>
    /// Formats a TimeOnly value according to the specified DateTimeHandling strategy.
    /// </summary>
    /// <param name="timeValue">The time value to format.</param>
    /// <param name="dateTimeHandling">The formatting strategy to apply.</param>
    /// <returns>The formatted time string.</returns>
    public static string FormatTime(TimeOnly timeValue, DateTimeHandling dateTimeHandling) {
        var formatString = GetFormatString(dateTimeHandling);

        return dateTimeHandling switch {
            DateTimeHandling.Utc => timeValue.ToString(formatString),
            DateTimeHandling.Local => timeValue.ToString(formatString),
            DateTimeHandling.Unspecified => timeValue.ToString("s"),
            _ => timeValue.ToString(formatString), // Round-trip format
        };
    }

    /// <summary>
    /// Gets the appropriate format string for the specified DateTimeHandling strategy.
    /// </summary>
    /// <param name="dateTimeHandling">The date/time handling strategy.</param>
    /// <returns>The format string to use for time formatting.</returns>
    public static string GetFormatString(DateTimeHandling dateTimeHandling) {
        return dateTimeHandling switch {
            DateTimeHandling.Utc => "s",
            DateTimeHandling.Local => "s",
            DateTimeHandling.Unspecified => "s",
            _ => "O", // Round-trip format
        };
    }
}
