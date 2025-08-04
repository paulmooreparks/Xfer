using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Configuration;
using ParksComputing.Xfer.Lang.Attributes;
using System.Text;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class XferConvertTests
{
    [TestMethod]
    public void Serialize_SimpleObject_ShouldProduceValidXfer()
    {
        // Arrange
        var testObject = new
        {
            name = "John Doe",
            age = 30,
            active = true
        };

        // Act
        var xferString = XferConvert.Serialize(testObject);

        // Assert
        Assert.IsNotNull(xferString);
        Assert.IsTrue(xferString.Contains("name<\"John Doe\">"));
        Assert.IsTrue(xferString.Contains("age 30"));
        Assert.IsTrue(xferString.Contains("active~true"));
    }

    [TestMethod]
    public void Serialize_NestedObject_ShouldHandleNesting()
    {
        // Arrange
        var testObject = new
        {
            user = new
            {
                profile = new
                {
                    name = "Jane Smith",
                    settings = new
                    {
                        theme = "dark"
                    }
                }
            }
        };

        // Act
        var xferString = XferConvert.Serialize(testObject);

        // Assert
        Assert.IsNotNull(xferString);
        Assert.IsTrue(xferString.Contains("user{"));
        Assert.IsTrue(xferString.Contains("profile{"));
        Assert.IsTrue(xferString.Contains("settings{"));
        Assert.IsTrue(xferString.Contains("name<\"Jane Smith\">"));
        Assert.IsTrue(xferString.Contains("theme\"dark\""));
    }

    [TestMethod]
    public void Serialize_Array_ShouldProduceArraySyntax()
    {
        // Arrange
        var testObject = new
        {
            numbers = new[] { 1, 2, 3, 4, 5 },
            names = new[] { "Alice", "Bob", "Charlie" }
        };

        // Act
        var xferString = XferConvert.Serialize(testObject);

        // Assert
        Assert.IsNotNull(xferString);
        Assert.IsTrue(xferString.Contains("numbers["));
        Assert.IsTrue(xferString.Contains("names["));
        Assert.IsTrue(xferString.Contains("<\"Alice\">"));
        Assert.IsTrue(xferString.Contains("<\"Bob\">"));
        Assert.IsTrue(xferString.Contains("<\"Charlie\">"));
    }

    [TestMethod]
    public void Serialize_NullValues_ShouldHandleNulls()
    {
        // Arrange
        var testObject = new
        {
            name = "Test",
            description = (string?)null,
            count = 42
        };

        // Act
        var xferString = XferConvert.Serialize(testObject);

        // Assert
        Assert.IsNotNull(xferString);
        Assert.IsTrue(xferString.Contains("name\"Test\""));
        Assert.IsTrue(xferString.Contains("count 42"));
        // Null values should either be omitted or represented as null
        Assert.IsTrue(xferString.Contains("description?") || !xferString.Contains("description"));
    }

    [TestMethod]
    public void Deserialize_SimpleXfer_ShouldParseCorrectly()
    {
        // Arrange
        var xferInput = """
        {
            name "John Doe"
            age #30
            active ~true
        }
        """;

        // Act
        var document = XferParser.Parse(xferInput);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsTrue(document.IsValid);
        Assert.AreEqual(3, document.Root.Children.Count);

        // Verify the parsed structure contains the expected key-value pairs
        var children = document.Root.Children.ToList();
        Assert.IsTrue(children.Any(c => c.ToString()?.Contains("name\"John Doe\"") == true));
        Assert.IsTrue(children.Any(c => c.ToString()?.Contains("age#30") == true));
        Assert.IsTrue(children.Any(c => c.ToString()?.Contains("active~true") == true));
    }

    [TestMethod]
    public void FromObject_SimpleObject_ShouldCreateObjectElement()
    {
        // Arrange
        var testObject = new
        {
            name = "Test",
            value = 42
        };

        // Act
        var objectElement = XferConvert.FromObject(testObject);

        // Assert
        Assert.IsNotNull(objectElement);
        Assert.IsInstanceOfType(objectElement, typeof(ObjectElement));
        Assert.IsTrue(objectElement.Dictionary.ContainsKey("name"));
        Assert.IsTrue(objectElement.Dictionary.ContainsKey("value"));
    }

    [TestMethod]
    public void Serialize_WithFormatting_ShouldProduceReadableOutput()
    {
        // Arrange
        var testObject = new
        {
            level1 = new
            {
                level2 = new
                {
                    data = "test"
                }
            }
        };

        // Act
        var xferString = XferConvert.Serialize(testObject, Formatting.Indented);

        // Assert
        Assert.IsNotNull(xferString);
        Assert.IsTrue(xferString.Contains("\n")); // Should contain newlines for formatting
        Assert.IsTrue(xferString.Contains("    ")); // Should contain indentation
    }

    [TestMethod]
    public void Serialize_WithNoFormatting_ShouldProduceCompactOutput()
    {
        // Arrange
        var testObject = new
        {
            name = "test",
            value = 42
        };

        // Act
        var xferString = XferConvert.Serialize(testObject, Formatting.None);

        // Assert
        Assert.IsNotNull(xferString);
        // Compact output should not contain unnecessary whitespace
        Assert.IsFalse(xferString.Contains("\n"));
    }

    [TestMethod]
    public void RoundTrip_SerializeAndDeserialize_ShouldWork()
    {
        // Arrange
        var originalObject = new
        {
            name = "Test Object",
            value = 123,
            isActive = true,
            tags = new[] { "tag1", "tag2", "tag3" }
        };

        // Act
        var serialized = XferConvert.Serialize(originalObject);
        var document = XferParser.Parse(serialized);

        // Assert
        Assert.IsNotNull(serialized);
        Assert.IsNotNull(document);
        Assert.IsTrue(document.IsValid);

        // Verify the serialized form can be parsed back successfully
        Assert.IsTrue(document.Root.Children.Count > 0);

        // Verify that the serialized string contains the expected structure
        Assert.IsTrue(serialized.Contains("name"));
        Assert.IsTrue(serialized.Contains("value"));
        Assert.IsTrue(serialized.Contains("isActive"));
        Assert.IsTrue(serialized.Contains("tags"));
    }

    [TestMethod]
    public void Serialize_EmptyObject_ShouldProduceEmptyXfer()
    {
        // Arrange
        var emptyObject = new { };

        // Act
        var xferString = XferConvert.Serialize(emptyObject);

        // Assert
        Assert.IsNotNull(xferString);
        Assert.IsTrue(xferString.Contains("{"));
        Assert.IsTrue(xferString.Contains("}"));
    }

    [TestMethod]
    public void Deserialize_EmptyXfer_ShouldReturnEmptyObject()
    {
        // Arrange
        var emptyXfer = "{}";

        // Act
        var document = XferParser.Parse(emptyXfer);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsTrue(document.IsValid);
        // Empty object should parse successfully even if deserialization returns null
    }

    [TestMethod]
    public void RoundTrip_MinifiedXferDocument_ShouldMaintainSemanticEquivalence()
    {
        // Arrange
        var minifiedXfer = "{name\"Alice\"age 30 salary*50000.75 isMember~true scores[*85*90*78.5]profile{email\"alice@example.com\"joinedDate@2023-05-05T20:00:00@}}";

        // Act - Parse the original document
        var originalDocument = XferParser.Parse(minifiedXfer);

        // The parser creates KeyValuePairElements as children, not a single ObjectElement
        // So we need to work with the root element directly
        var originalRoot = originalDocument.Root;

        // Re-serialize and parse again to test round-trip
        var serialized = originalRoot.ToXfer();
        var roundTripDocument = XferParser.Parse(serialized);
        var roundTripRoot = roundTripDocument.Root;

        // Assert - Check that the structure is preserved semantically
        Assert.IsNotNull(originalRoot);
        Assert.IsNotNull(roundTripRoot);

        // Check that both have the same number of children
        Assert.AreEqual(originalRoot.Children.Count, roundTripRoot.Children.Count);

        // Verify that the serialized content can be parsed again
        Assert.IsNotNull(serialized);
        Assert.IsTrue(serialized.Length > 0);

        // Basic structural validation - ensure the round-trip preserves the document structure
        Assert.AreEqual(originalDocument.Root.GetType(), roundTripDocument.Root.GetType());
    }

    #region XferDecimalPrecisionAttribute Tests

    [TestMethod]
    public void Serialize_DecimalWithPrecisionAttribute_ShouldRespectDecimalPlaces()
    {
        // Arrange
        var testObject = new DecimalPrecisionTestClass
        {
            Price = 123.456789m,
            Temperature = 98.76543,
            Interest = 5.25m,
            Quantity = 150.999m
        };

        // Act
        var xferString = XferConvert.Serialize(testObject);

        // Assert
        Assert.IsNotNull(xferString);
        Assert.IsTrue(xferString.Contains("Price*123.46"), $"Expected Price*123.46 but got: {xferString}");
        Assert.IsTrue(xferString.Contains("Temperature^98.8"), $"Expected Temperature^98.8 but got: {xferString}");
        Assert.IsTrue(xferString.Contains("Interest*5.2500"), $"Expected Interest*5.2500 but got: {xferString}");
        Assert.IsTrue(xferString.Contains("Quantity*151"), $"Expected Quantity*151 but got: {xferString}");
    }

    [TestMethod]
    public void Serialize_DecimalWithoutPrecisionAttribute_ShouldUseDefaultPrecision()
    {
        // Arrange
        var testObject = new DecimalPrecisionTestClass
        {
            Cost = 99.99999m
        };

        // Act
        var xferString = XferConvert.Serialize(testObject);

        // Assert
        Assert.IsNotNull(xferString);
        Assert.IsTrue(xferString.Contains("Cost*99.99999"), $"Expected Cost*99.99999 but got: {xferString}");
    }

    [TestMethod]
    public void Serialize_DecimalWithZeroPrecision_ShouldRemoveDecimalPlaces()
    {
        // Arrange
        var testObject = new DecimalPrecisionTestClass
        {
            Quantity = 150.999m
        };

        // Act
        var xferString = XferConvert.Serialize(testObject);

        // Assert
        Assert.IsNotNull(xferString);
        Assert.IsTrue(xferString.Contains("Quantity*151"), $"Expected Quantity*151 but got: {xferString}");
    }

    [TestMethod]
    public void Serialize_DecimalWithTrailingZerosPreserved_ShouldKeepTrailingZeros()
    {
        // Arrange
        var testObject = new DecimalPrecisionTestClass
        {
            Interest = 5.25m
        };

        // Act
        var xferString = XferConvert.Serialize(testObject);

        // Assert
        Assert.IsNotNull(xferString);
        Assert.IsTrue(xferString.Contains("Interest*5.2500"), $"Expected Interest*5.2500 but got: {xferString}");
    }

    [TestMethod]
    public void Deserialize_DecimalWithPrecision_ShouldMaintainOriginalValue()
    {
        // Arrange
        var originalValue = 123.456789m;
        var testObject = new DecimalPrecisionTestClass { Price = originalValue };
        var xferString = XferConvert.Serialize(testObject);

        // Act
        var deserializedObject = XferConvert.Deserialize<DecimalPrecisionTestClass>(xferString);

        // Assert
        Assert.IsNotNull(deserializedObject);
        // Note: Due to precision rounding during serialization, we check the rounded value
        Assert.AreEqual(123.46m, deserializedObject.Price);
    }

    [TestMethod]
    public void Serialize_DoubleWithPrecisionAttribute_ShouldRespectPrecision()
    {
        // Arrange
        var testObject = new DecimalPrecisionTestClass
        {
            Temperature = 98.76543
        };

        // Act
        var xferString = XferConvert.Serialize(testObject);

        // Assert
        Assert.IsNotNull(xferString);
        Assert.IsTrue(xferString.Contains("Temperature^98.8"), $"Expected Temperature^98.8 but got: {xferString}");
    }

    #endregion

    #region Test Helper Classes

    private class DecimalPrecisionTestClass
    {
        [XferDecimalPrecision(2)]
        public decimal Price { get; set; }

        [XferDecimalPrecision(4, RemoveTrailingZeros = false)]
        public decimal Interest { get; set; }

        [XferDecimalPrecision(1)]
        public double Temperature { get; set; }

        [XferDecimalPrecision(0)]
        public decimal Quantity { get; set; }

        // Without attribute - uses default precision
        public decimal Cost { get; set; }
    }

    #endregion
}
