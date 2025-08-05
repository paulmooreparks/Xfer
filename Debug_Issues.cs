using System;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Configuration;
using ParksComputing.Xfer.Lang.Attributes;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Converters;

namespace Debug
{
    public class DebugTest
    {
        public static void Main()
        {
            // Test 1: Basic numeric formatting
            var numericObj = new NumericTestClass
            {
                DecimalValue = 42,
                HexValue = 255,
                BinaryValue = 42
            };

            string result1 = XferConvert.Serialize(numericObj);
            Console.WriteLine($"Numeric formatting test: {result1}");

            // Test 2: DateTime precision
            var dt = new DateTime(2023, 12, 25, 10, 30, 45, 123);
            var settings = new XferSerializerSettings { PreserveDateTimePrecision = true };
            string result2 = XferConvert.Serialize(dt, settings);
            Console.WriteLine($"DateTime with precision: {result2}");

            // Test 3: Custom converter
            var person = new Person { Name = "John", Age = 30 };
            var settingsWithConverter = new XferSerializerSettings();
            settingsWithConverter.Converters.Add(new PersonConverter());
            string result3 = XferConvert.Serialize(person, settingsWithConverter);
            Console.WriteLine($"Custom converter: {result3}");

            // Test 4: Property attributes
            var propObj = new PropertyTestClass
            {
                RegularProperty = "normal",
                CustomName = "custom"
            };
            string result4 = XferConvert.Serialize(propObj);
            Console.WriteLine($"Property attributes: {result4}");

            // Test 5: Deserialization test
            try
            {
                var deserializedPerson = XferConvert.Deserialize<Person>("\"John,30\"", settingsWithConverter);
                Console.WriteLine($"Deserialized person: {deserializedPerson?.Name}, {deserializedPerson?.Age}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deserialization error: {ex.Message}");
            }
        }
    }

    public class NumericTestClass
    {
        [XferNumericFormat(XferNumericFormat.Decimal)]
        public int DecimalValue { get; set; }

        [XferNumericFormat(XferNumericFormat.Hexadecimal)]
        public int HexValue { get; set; }

        [XferNumericFormat(XferNumericFormat.Binary)]
        public int BinaryValue { get; set; }
    }

    public class PropertyTestClass
    {
        public string RegularProperty { get; set; } = "";

        [XferProperty("custom_name")]
        public string CustomName { get; set; } = "";
    }

    public class Person
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }
}
