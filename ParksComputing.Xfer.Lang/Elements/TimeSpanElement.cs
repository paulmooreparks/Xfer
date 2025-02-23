using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

public class TimeSpanElement : TypedElement<TimeSpan> {
    public static readonly string ElementName = DateTimeElement.ElementName;
    public const char OpeningSpecifier = DateTimeElement.OpeningSpecifier;
    public const char ClosingSpecifier = DateTimeElement.ClosingSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = DateTimeElement.ElementDelimiter;

    public DateTimeHandling DateTimeHandling { get; set; } = DateTimeHandling.RoundTrip;

    public TimeSpanElement(DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
        : this(TimeSpan.Zero, dateTimeHandling, specifierCount, elementStyle) {
    }

    public TimeSpanElement(string stringValue, DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
        : this(TimeSpan.Zero, dateTimeHandling, specifierCount, elementStyle) {
        if (!TimeSpan.TryParse(stringValue, out var timeSpan)) {
            throw new InvalidOperationException($"Invalid time string '{stringValue}'. Expected ISO 8601 format.");
        }

        Value = timeSpan;
        DateTimeHandling = dateTimeHandling;
    }

    public TimeSpanElement(TimeSpan timeSpan, DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
        : base(timeSpan, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier)) {
        DateTimeHandling = dateTimeHandling;
    }

    public override string ToXfer() {
        return ToXfer(Formatting.None);
    }

    public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0) {
        var sb = new StringBuilder();
        sb.Append($"{Delimiter.MinOpening}{ToString()}{Delimiter.MinClosing}");
        return sb.ToString();
    }

    public override string ToString() {
        string timeSpanValue = FormatTimeSpan(Value, DateTimeHandling);
        return timeSpanValue;
    }

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

    public static string GetFormatString(DateTimeHandling dateTimeHandling) {
        return dateTimeHandling switch {
            DateTimeHandling.Utc => "s",
            DateTimeHandling.Local => "s",
            DateTimeHandling.Unspecified => "s",
            _ => "O", // Round-trip format
        };
    }
}
