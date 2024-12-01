﻿using System;
using System.Globalization;

namespace ParksComputing.Xfer.Models.Elements
{
    public class DateElement : TypedElement<DateTime> {
        public static readonly string ElementName = "date";
        public const char OpeningSpecifier = '@';
        public const char ClosingSpecifier = OpeningSpecifier;
        public static readonly ElementDelimiter ElementDelimiter = new ElementDelimiter(OpeningSpecifier, ClosingSpecifier);

        public DateElement(string input, int specifierCount = 1, ElementStyle elementStyle = ElementStyle.Compact) 
            : base(DateTime.Now, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier, specifierCount, elementStyle)) 
        {
            if (!DateTime.TryParseExact(input, new[] { "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd" }, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime dateValue)) {
                throw new InvalidOperationException($"Invalid date format '{input}'. Expected ISO 8601 format: 'yyyy-MM-ddTHH:mm:ss' or 'yyyy-MM-dd'.");
            }

            Value = dateValue;
        }

        public DateElement(DateTime dateValue) : base(dateValue, ElementName, new ElementDelimiter(OpeningSpecifier, ClosingSpecifier)) {
        }

        public override string ToXfer() {
            return $"{Delimiter.MinOpening}{ToString()}{Delimiter.MinClosing}";
        }

        public override string ToString() {
            string dateValue = Value.TimeOfDay == TimeSpan.Zero
                    ? $"{Value:yyyy-MM-dd}"
                    : $"{Value:yyyy-MM-ddTHH:mm:ss}";
            return dateValue;
        }
    }
}
