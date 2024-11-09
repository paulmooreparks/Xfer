using System;
using System.Globalization;

using ParksComputing.Xfer.Models.Elements;

namespace ParksComputing.Xfer.Models.Elements {
    public class DateElement : Element {
        public static readonly string ElementName = "date";
        public const char OpeningMarker = '@';
        public const char ClosingMarker = OpeningMarker;

        public DateTime TypedValue { get; }
        public override string Value {
            get {
                return TypedValue.TimeOfDay == TimeSpan.Zero
                    ? $"{TypedValue:yyyy-MM-dd}"
                    : $"{TypedValue:yyyy-MM-ddTHH:mm:ss}";
            }
        }

        public DateElement(string input) : base(ElementName, new Delimiter(OpeningMarker, ClosingMarker)) {
            if (!DateTime.TryParseExact(input, new[] { "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd" }, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime dateValue)) {
                throw new InvalidOperationException($"Invalid date format '{input}'. Expected ISO 8601 format: 'yyyy-MM-ddTHH:mm:ss' or 'yyyy-MM-dd'.");
            }

            TypedValue = dateValue;
        }

        public DateElement(DateTime dateValue) : base(ElementName, new Delimiter(OpeningMarker, ClosingMarker)) {
            TypedValue = dateValue;
        }

        public override string ToString() {
            return TypedValue.TimeOfDay == TimeSpan.Zero
                ? $"{Delimiter.Opening}{Value:yyyy-MM-dd}{Delimiter.Closing}"
                : $"{Delimiter.Opening}{Value:yyyy-MM-ddTHH:mm:ss}{Delimiter.Closing}";
        }
    }
}
