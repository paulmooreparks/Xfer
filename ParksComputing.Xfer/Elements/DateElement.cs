using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Elements;

public class DateElement : TypedElement<DateOnly> {
    public static readonly string ElementName = DateTimeElement.ElementName;
    public const char OpeningSpecifier = DateTimeElement.OpeningSpecifier;
    public const char ClosingSpecifier = DateTimeElement.ClosingSpecifier;
    public static readonly ElementDelimiter ElementDelimiter = DateTimeElement.ElementDelimiter;

    public DateTimeHandling DateTimeHandling { get; set; } = DateTimeHandling.RoundTrip;

    public DateElement(DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
        : this(DateOnly.FromDateTime(DateTime.Now), dateTimeHandling, specifierCount, elementStyle) {
    }

    public DateElement(string stringValue, DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
        : this(DateOnly.FromDateTime(DateTime.Now), dateTimeHandling, specifierCount, elementStyle) {
        if (!DateOnly.TryParse(stringValue, out var dateOnly)) {
            throw new InvalidOperationException($"Invalid date string '{stringValue}'. Expected ISO 8601 format.");
        }

        Value = dateOnly;
        DateTimeHandling = dateTimeHandling;
    }

    public DateElement(DateOnly dateOnly, DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
        : base(dateOnly, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier)) {
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
        string dateValue = FormatDate(Value, DateTimeHandling);
        return dateValue;
    }

    public static string FormatDate(DateOnly dateValue, DateTimeHandling dateTimeHandling) {
        var formatString = GetFormatString(dateTimeHandling);

        return dateTimeHandling switch {
            DateTimeHandling.Utc => dateValue.ToString(formatString),
            DateTimeHandling.Local => dateValue.ToString(formatString),
            DateTimeHandling.Unspecified => dateValue.ToString("O"),
            _ => dateValue.ToString(formatString), // Round-trip format
        };
    }

    public static string GetFormatString(DateTimeHandling dateTimeHandling) {
        return dateTimeHandling switch {
            DateTimeHandling.Utc => "O",
            DateTimeHandling.Local => "O",
            DateTimeHandling.Unspecified => "O",
            _ => "O", // Round-trip format
        };
    }
}
