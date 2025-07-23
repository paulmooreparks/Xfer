using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
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
}

public class LowerCaseContractResolver : ParksComputing.Xfer.Lang.ContractResolvers.DefaultContractResolver
{
    public override string ResolvePropertyName(string propertyName)
    {
        return propertyName.ToLower();
    }
}
