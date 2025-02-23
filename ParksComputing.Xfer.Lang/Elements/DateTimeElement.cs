using System;
using System.Globalization;
using System.Text;

namespace ParksComputing.Xfer.Lang.Elements
{
    public class DateTimeElement : TypedElement<DateTime>
    {
        public static readonly string ElementName = "date";
        public const char OpeningSpecifier = '@';
        public const char ClosingSpecifier = OpeningSpecifier;
        public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

        public DateTimeHandling DateTimeHandling { get; set; } = DateTimeHandling.RoundTrip;

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

        public DateTimeElement(TimeOnly timeOnly, DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
            : this(timeOnly.ToLongTimeString(), dateTimeHandling, specifierCount, elementStyle) 
        {
            Value = new DateTime(0, 0, 0, timeOnly.Hour, timeOnly.Minute, timeOnly.Second);
        }

        public DateTimeElement(DateTime dateValue, DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact) : base(dateValue, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier))
        {
            DateTimeHandling = dateTimeHandling;
        }

        public override string ToXfer()
        {
            return ToXfer(Formatting.None);
        }

        public override string ToXfer(Formatting formatting, char indentChar = ' ', int indentation = 2, int depth = 0)
        {
            var sb = new StringBuilder();
            sb.Append($"{Delimiter.MinOpening}{ToString()}{Delimiter.MinClosing}");
            return sb.ToString();
        }

        public override string ToString()
        {
            string dateValue = FormatDate(Value, DateTimeHandling);
            return dateValue;
        }

        public static string FormatDate(DateTime dateValue, DateTimeHandling dateTimeHandling) {
            var formatString = GetFormatString(dateTimeHandling);

            return dateTimeHandling switch {
                DateTimeHandling.Utc => dateValue.ToUniversalTime().ToString(formatString),
                DateTimeHandling.Local => dateValue.ToLocalTime().ToString(formatString),
                DateTimeHandling.Unspecified => dateValue.ToString(formatString),
                _ => dateValue.ToString(formatString), // Round-trip format
            };
        }

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
