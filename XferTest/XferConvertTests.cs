using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Attributes;
using ParksComputing.Xfer.Lang.Configuration;
using System;
using System.Collections.Generic;

namespace XferTest;

[TestClass]
public class XferConvertTests
{
    public class SampleData
    {
        public string StringProperty { get; set; } = "Hello, World!";
        public int IntProperty { get; set; } = 42;
        public bool BoolProperty { get; set; } = true;
        public double DoubleProperty { get; set; } = 3.14159;
        public DateTime DateTimeProperty { get; set; } = new DateTime(2025, 7, 23, 12, 30, 0);
        public Guid GuidProperty { get; set; } = Guid.NewGuid();
        public List<string> ListProperty { get; set; } = new List<string> { "one", "two", "three" };
        public Dictionary<string, int> DictionaryProperty { get; set; } = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
        public string? NullStringProperty { get; set; } = null;
    }

    [TestMethod]
    public void SerializeAndDeserialize_ComplexObject_ShouldPreserveValues()
    {
        // Arrange
        var original = new SampleData();

        // Act
        string xfer = XferConvert.Serialize(original, Formatting.Indented);
        var deserialized = XferConvert.Deserialize<SampleData>(xfer);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.StringProperty, deserialized.StringProperty);
        Assert.AreEqual(original.IntProperty, deserialized.IntProperty);
        Assert.AreEqual(original.BoolProperty, deserialized.BoolProperty);
        Assert.AreEqual(original.DoubleProperty, deserialized.DoubleProperty);
        Assert.AreEqual(original.DateTimeProperty, deserialized.DateTimeProperty);
        Assert.AreEqual(original.GuidProperty, deserialized.GuidProperty);
        CollectionAssert.AreEqual(original.ListProperty, deserialized.ListProperty);
        CollectionAssert.AreEqual(original.DictionaryProperty, deserialized.DictionaryProperty);
        Assert.IsNull(deserialized.NullStringProperty);
    }

    [TestMethod]
    public void Serialize_WithNullValueHandlingIgnore_ShouldOmitNullProperties()
    {
        // Arrange
        var data = new SampleData
        {
            NullStringProperty = null
        };
        var settings = new XferSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        // Act
        string xfer = XferConvert.Serialize(data, settings);

        // Assert
        Assert.IsFalse(xfer.Contains(nameof(SampleData.NullStringProperty)));
    }

    [TestMethod]
    public void Serialize_WithCustomContractResolver_ShouldUseResolver()
    {
        // Arrange
        var data = new SampleData();
        var settings = new XferSerializerSettings
        {
            ContractResolver = new LowerCaseContractResolver()
        };

        // Act
        string xfer = XferConvert.Serialize(data, settings);

        // Assert
        Assert.IsTrue(xfer.Contains(nameof(SampleData.StringProperty).ToLower()));
        Assert.IsFalse(xfer.Contains(nameof(SampleData.StringProperty)));
    }

    [TestMethod]
    public void SerializeAndDeserialize_WithCustomConverter_ShouldUseConverter()
    {
        // Arrange
        var original = new ParksComputing.Xfer.Lang.Models.Person { Name = "John Doe", Age = 42 };
        var settings = new XferSerializerSettings();
        settings.Converters.Add(new ParksComputing.Xfer.Lang.Converters.PersonConverter());

        // Act
        string xfer = XferConvert.Serialize(original, settings);
        var deserialized = XferConvert.Deserialize<ParksComputing.Xfer.Lang.Models.Person>(xfer, settings);

        // Assert
        Assert.AreEqual("\"John Doe,42\"", xfer.Trim());
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.Name, deserialized.Name);
        Assert.AreEqual(original.Age, deserialized.Age);
    }

    [TestMethod]
    public void ParseAndRoundTrip_MinifiedXferDocument_ShouldPreserveStructure()
    {
        // Arrange - the minified Xfer document from the documentation
        string minifiedXfer = "{name\"Alice\"age 30 isMember~true scores[*85*90*78.5]profile{email\"alice@example.com\"joinedDate@2023-05-05T20:00:00@}}";

        // Create expected object for comparison
        var expected = new UserData
        {
            Name = "Alice",
            Age = 30,
            IsMember = true,
            Scores = new decimal[] { 85m, 90m, 78.5m },
            Profile = new UserProfile
            {
                Email = "alice@example.com",
                JoinedDate = new DateTime(2023, 5, 5, 20, 0, 0, DateTimeKind.Unspecified)
            }
        };

        // Act - Parse the minified document
        var parsed = XferConvert.Deserialize<UserData>(minifiedXfer);

        // Round-trip: serialize back to Xfer with compact preferences
        var compactSettings = new XferSerializerSettings
        {
            StylePreference = ElementStylePreference.CompactWhenSafe,
            PreferImplicitSyntax = true
        };
        string compactSerialized = XferConvert.Serialize(parsed, compactSettings, Formatting.None);
        Console.WriteLine($"Compact serialized: {compactSerialized}");

        // Round-trip: serialize back with default (explicit) settings
        string defaultSerialized = XferConvert.Serialize(parsed, Formatting.None);
        Console.WriteLine($"Default serialized: {defaultSerialized}");

        var roundTripped = XferConvert.Deserialize<UserData>(compactSerialized);

        // Assert - Verify parsing worked correctly
        Assert.IsNotNull(parsed);
        Assert.AreEqual(expected.Name, parsed.Name);
        Assert.AreEqual(expected.Age, parsed.Age);
        Assert.AreEqual(expected.IsMember, parsed.IsMember);
        CollectionAssert.AreEqual(expected.Scores, parsed.Scores);
        Assert.IsNotNull(parsed.Profile);
        Assert.AreEqual(expected.Profile.Email, parsed.Profile.Email);
        Assert.AreEqual(expected.Profile.JoinedDate, parsed.Profile.JoinedDate);

        // Assert - Verify round-trip worked correctly
        Assert.IsNotNull(roundTripped);
        Assert.AreEqual(expected.Name, roundTripped.Name);
        Assert.AreEqual(expected.Age, roundTripped.Age);
        Assert.AreEqual(expected.IsMember, roundTripped.IsMember);
        CollectionAssert.AreEqual(expected.Scores, roundTripped.Scores);
        Assert.IsNotNull(roundTripped.Profile);
        Assert.AreEqual(expected.Profile.Email, roundTripped.Profile.Email);
        Assert.AreEqual(expected.Profile.JoinedDate, roundTripped.Profile.JoinedDate);

        // Assert - Verify compact serialization is more similar to original
        Assert.IsTrue(compactSerialized.Contains("name\"Alice\""), "Compact should use quoted strings when safe");
        Assert.IsTrue(compactSerialized.Contains("email\"alice@example.com\""), "Safe email should use compact quotes");

        // Assert - Verify default serialization uses compact-when-safe style (new default)
        Assert.IsTrue(defaultSerialized.Contains("name\"Alice\""), "Default should use compact style for safe strings");
    }

    [TestMethod]
    public void Serialize_FormattingInconsistencies_DemonstrateCurrentLimitations()
    {
        // Arrange - simple data that should serialize compactly
        var data = new { name = "Alice", age = 30, active = true };

        // Act
        string serialized = XferConvert.Serialize(data, Formatting.None);

        // This test documents current default behavior (compact when safe, implicit for integers)
        // Current output: {name"Alice"age 30 active~true}
        // With explicit preference: {name<"Alice">age<#30#>active~true}

        // Assert - Document current default behavior
        Assert.IsTrue(serialized.Contains("name\"Alice\""), "Safe strings use compact style by default for readability");
        Assert.IsTrue(serialized.Contains("age 30") || serialized.Contains("age30"), "Decimal integers use implicit style by default (always safe)");
        Assert.IsTrue(serialized.Contains("active~true"), "Booleans use compact style");

        // More explicit styles are available via configuration:
        // ElementStylePreference.Explicit would produce: {name<"Alice">age<#30#>active~true}
    }

    [TestMethod]
    public void Serialize_WithCompactWhenSafePreference_ShouldUseCompactStrings()
    {
        // Arrange - data with safe and unsafe strings
        var data = new
        {
            safeName = "Alice",          // No quotes, whitespace, or special chars - safe for compact
            unsafeName = "Alice \"Bob\"",  // Contains quotes - needs explicit
            simpleAge = 30,              // Simple integer
            message = "Hello World"      // Contains space - needs explicit
        };

        var settings = new XferSerializerSettings
        {
            StylePreference = ElementStylePreference.CompactWhenSafe
        };

        // Act
        string serialized = XferConvert.Serialize(data, settings, Formatting.None);
        Console.WriteLine($"Serialized output: {serialized}");

        // Assert - Should use compact for safe strings, explicit for unsafe
        Assert.IsTrue(serialized.Contains("safeName\"Alice\""), "Safe strings should use compact style");
        Assert.IsTrue(serialized.Contains("unsafeName<"), "Unsafe strings should use explicit style");
        Assert.IsTrue(serialized.Contains("simpleAge") && serialized.Contains("30"), "Should contain age");
        Assert.IsTrue(serialized.Contains("message<"), "Strings with spaces should use explicit style");
    }

    [TestMethod]
    public void Serialize_WithMinimalWhenSafePreference_ShouldUseImplicitSyntax()
    {
        // Arrange - simple data suitable for implicit syntax
        var data = new
        {
            name = "Alice",
            age = 42,
            count = 0,
            active = true
        };

        var settings = new XferSerializerSettings
        {
            StylePreference = ElementStylePreference.MinimalWhenSafe,
            PreferImplicitSyntax = true
        };

        // Act
        string serialized = XferConvert.Serialize(data, settings, Formatting.None);

        // Assert - Should use most minimal syntax when safe
        Assert.IsTrue(serialized.Contains("name\"Alice\""), "Safe strings should use compact style");
        Assert.IsTrue(serialized.Contains("age 42") || serialized.Contains("age42"), "Simple integers should use implicit style");
        Assert.IsTrue(serialized.Contains("count 0") || serialized.Contains("count0"), "Zero should use implicit style");
        Assert.IsTrue(serialized.Contains("active~true"), "Booleans should still use compact style");
    }

    [TestMethod]
    public void Serialize_ExplicitStylePreference_ShouldAlwaysUseExplicitForSafety()
    {
        // Arrange - even simple data
        var data = new { name = "Alice", age = 30 };

        var settings = new XferSerializerSettings
        {
            StylePreference = ElementStylePreference.Explicit  // Default - maximum safety
        };

        // Act
        string serialized = XferConvert.Serialize(data, settings, Formatting.None);

        // Assert - Should always use explicit/safe styles
        Assert.IsTrue(serialized.Contains("name<\"Alice\">"), "Should use explicit style for strings");
        Assert.IsTrue(serialized.Contains("age<#30#>"), "Should use explicit style for integers when explicitly requested");
    }

    [TestMethod]
    public void Serialize_NumericTypes_ShouldPreserveTypeInformation()
    {
        // Arrange - same value (42) in different numeric types
        var data = new
        {
            intValue = 42,           // Should use implicit: 42
            longValue = 42L,         // Should use compact: &42
            doubleValue = 42.0,      // Should use compact: ^42.0
            decimalValue = 42.0m     // Should use compact: *42.0
        };

        // Act
        string serialized = XferConvert.Serialize(data, Formatting.None);

        // Assert - Each type should preserve type information except int
        Assert.IsTrue(serialized.Contains("intValue 42") || serialized.Contains("intValue42"), "int should use implicit style (no type marker)");
        Assert.IsTrue(serialized.Contains("longValue&42"), "long should use compact style to preserve type information");
        Assert.IsTrue(serialized.Contains("doubleValue^42"), "double should use compact style to preserve type information");
        Assert.IsTrue(serialized.Contains("decimalValue*42"), "decimal should use compact style to preserve type information");
    }

    [TestMethod]
    public void Serialize_NumericTypesWithExplicitPreference_ShouldUseExplicitStyle()
    {
        // Arrange - same value in different numeric types with explicit preference
        var data = new
        {
            intValue = 42,
            longValue = 42L,
            doubleValue = 42.0,
            decimalValue = 42.0m
        };

        var settings = new XferSerializerSettings
        {
            StylePreference = ElementStylePreference.Explicit
        };

        // Act
        string serialized = XferConvert.Serialize(data, settings, Formatting.None);

        // Assert - All numeric types should use explicit style when requested
        Assert.IsTrue(serialized.Contains("intValue<#42#>"), "int should use explicit style when requested");
        Assert.IsTrue(serialized.Contains("longValue<&42&>"), "long should use explicit style when requested");
        Assert.IsTrue(serialized.Contains("doubleValue<^42^>"), "double should use explicit style when requested");
        Assert.IsTrue(serialized.Contains("decimalValue<*42.0*>"), "decimal should use explicit style when requested");
    }

    [TestMethod]
    public void Serialize_NumericFormattingWithAttributes_ShouldUseCustomFormats()
    {
        // Arrange - class with numeric formatting attributes
        var data = new NumericFormattingTestClass
        {
            DecimalValue = 42,
            HexValue = 42,
            BinaryValue = 42,
            HexWithMinDigits = 42,
            BinaryWithMinBits = 42
        };

        // Act
        string serialized = XferConvert.Serialize(data, Formatting.None);

        // Assert - Should use custom formatting based on attributes
        Assert.IsTrue(serialized.Contains("DecimalValue 42") || serialized.Contains("DecimalValue42"), "Decimal format should use implicit/compact style");
        Assert.IsTrue(serialized.Contains("HexValue #$2A"), "Hex format should use #$ prefix");
        Assert.IsTrue(serialized.Contains("BinaryValue #%101010"), "Binary format should use #% prefix");
        Assert.IsTrue(serialized.Contains("HexWithMinDigits #$002A"), "Hex with min digits should pad with zeros");
        Assert.IsTrue(serialized.Contains("BinaryWithMinBits #%00101010"), "Binary with min bits should pad with zeros");
    }

    [TestMethod]
    public void SerializeObject_WithDecimalAndDoubleProperties_UsesCompactStyle()
    {
        // Arrange
        var obj = new
        {
            DecimalValue = 123.45m,
            DoubleValue = 67.89,
            LargeDecimal = 999999.999m,
            SmallDouble = 0.001
        };

        var settings = new XferSerializerSettings();

        // Act
        string serialized = XferConvert.Serialize(obj, settings, Formatting.None);
        Console.WriteLine($"Decimal/Double serialized: {serialized}");

        // Assert - Should use compact style for these types
        Assert.IsTrue(serialized.Contains("DecimalValue*123.45"), "Decimal should use compact style with * prefix");
        Assert.IsTrue(serialized.Contains("DoubleValue^67.89"), "Double should use compact style with ^ prefix");
        Assert.IsTrue(serialized.Contains("LargeDecimal*999999.999"), "Large decimal should use compact style with * prefix");
        Assert.IsTrue(serialized.Contains("SmallDouble^0.001"), "Small double should use compact style with ^ prefix");
    }
}

/// <summary>
/// Test class for demonstrating numeric formatting attributes.
/// </summary>
public class NumericFormattingTestClass
{
    [XferNumericFormat(XferNumericFormat.Decimal)]
    public int DecimalValue { get; set; }

    [XferNumericFormat(XferNumericFormat.Hexadecimal)]
    public int HexValue { get; set; }

    [XferNumericFormat(XferNumericFormat.Binary)]
    public int BinaryValue { get; set; }

    [XferNumericFormat(XferNumericFormat.Hexadecimal, MinDigits = 4)]
    public int HexWithMinDigits { get; set; }

    [XferNumericFormat(XferNumericFormat.Binary, MinBits = 8)]
    public int BinaryWithMinBits { get; set; }
}

public class LowerCaseContractResolver : ParksComputing.Xfer.Lang.ContractResolvers.DefaultContractResolver
{
    public override string ResolvePropertyName(string propertyName)
    {
        return propertyName.ToLower();
    }
}

public class UserData
{
    [XferProperty("name")]
    public string Name { get; set; } = string.Empty;

    [XferProperty("age")]
    public int Age { get; set; }

    [XferProperty("isMember")]
    public bool IsMember { get; set; }

    [XferProperty("scores")]
    public decimal[] Scores { get; set; } = new decimal[0];

    [XferProperty("profile")]
    public UserProfile Profile { get; set; } = new UserProfile();
}

public class UserProfile
{
    [XferProperty("email")]
    public string Email { get; set; } = string.Empty;

    [XferProperty("joinedDate")]
    public DateTime JoinedDate { get; set; }
}
