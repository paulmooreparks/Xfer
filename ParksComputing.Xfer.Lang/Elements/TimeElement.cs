using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Lang.Elements;

public class TimeElement : TypedElement<TimeOnly> {
    public static readonly string ElementName = DateTimeElement.ElementName;
    public const char OpeningSpecifier = DateTimeElement.OpeningSpecifier;
    public const char ClosingSpecifier = DateTimeElement.ClosingSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = DateTimeElement.ElementDelimiter;

    public DateTimeHandling DateTimeHandling { get; set; } = DateTimeHandling.RoundTrip;

    public TimeElement(DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
        : this(TimeOnly.FromDateTime(DateTime.Now), dateTimeHandling, specifierCount, elementStyle) 
    {
    }

    public TimeElement(string stringValue, DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
        : this(TimeOnly.FromDateTime(DateTime.Now), dateTimeHandling, specifierCount, elementStyle) 
    {
        if (!TimeOnly.TryParse(stringValue, out var timeOnly)) {
            throw new InvalidOperationException($"Invalid time string '{stringValue}'. Expected ISO 8601 format.");
        }

        Value = timeOnly;
        DateTimeHandling = dateTimeHandling;
    }

    public TimeElement(TimeOnly timeOnly, DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact) 
        : base(timeOnly, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier)) 
    {
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
        string timeValue = FormatTime(Value, DateTimeHandling);
        return timeValue;
    }

    public static string FormatTime(TimeOnly timeValue, DateTimeHandling dateTimeHandling) {
        var formatString = GetFormatString(dateTimeHandling);

        return dateTimeHandling switch {
            DateTimeHandling.Utc => timeValue.ToString(formatString),
            DateTimeHandling.Local => timeValue.ToString(formatString),
            DateTimeHandling.Unspecified => timeValue.ToString("s"),
            _ => timeValue.ToString(formatString), // Round-trip format
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
