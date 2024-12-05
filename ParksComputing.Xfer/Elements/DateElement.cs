using System;
using System.Globalization;
using System.Text;

namespace ParksComputing.Xfer.Elements
{
    public class DateElement : TypedElement<DateTime>
    {
        public static readonly string ElementName = "date";
        public const char OpeningSpecifier = '@';
        public const char ClosingSpecifier = OpeningSpecifier;
        public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

        public DateTimeHandling DateTimeHandling { get; set; } = DateTimeHandling.RoundTrip;

        public DateElement(string input, DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact)
            : this(DateTime.Now, dateTimeHandling, specifierCount, elementStyle)
        {
            if (!DateTime.TryParse(input, out DateTime dateValue)) {
                if (!DateTime.TryParseExact(input, new[] { GetFormatString(DateTimeHandling), "yyyy-MM-dd", "HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out dateValue)) {
                    throw new InvalidOperationException($"Invalid date format '{input}'. Expected ISO 8601 format.");
                }
            }

            Value = dateValue;
        }

        public DateElement(DateTime dateValue, DateTimeHandling dateTimeHandling = DateTimeHandling.RoundTrip, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact) : base(dateValue, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier))
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
            string dateValue = Value.TimeOfDay == TimeSpan.Zero
                    ? $"{Value:yyyy-MM-dd}"
                    : FormatDate(Value, DateTimeHandling);
            return dateValue;
        }

        public static string FormatDate(DateTime dateValue, DateTimeHandling dateTimeHandling) {
            return dateTimeHandling switch {
                DateTimeHandling.Utc => dateValue.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                DateTimeHandling.Local => dateValue.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:sszzz"),
                DateTimeHandling.Unspecified => dateValue.ToString("yyyy-MM-ddTHH:mm:ss"),
                _ => dateValue.ToString("yyyy-MM-ddTHH:mm:sszzz"),
            };
        }

        public static string GetFormatString(DateTimeHandling dateTimeHandling) {
            return dateTimeHandling switch {
                DateTimeHandling.Utc => "yyyy-MM-ddTHH:mm:ssZ",
                DateTimeHandling.Local => "yyyy-MM-ddTHH:mm:sszzz",
                DateTimeHandling.Unspecified => "yyyy-MM-ddTHH:mm:ss",
                _ => "yyyy-MM-ddTHH:mm:sszzz",
            };
        }
    }
}
