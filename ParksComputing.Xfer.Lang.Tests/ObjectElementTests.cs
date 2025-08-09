using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.ProcessingInstructions;
using System;
using System.Linq;

namespace ParksComputing.Xfer.Lang.Tests;

/// <summary>
/// Comprehensive tests for ObjectElement functionality.
/// Tests dictionary-like behavior, key-value pair management, and XferLang serialization.
/// </summary>
[TestClass]
public class ObjectElementTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_Default_SetsCorrectly()
    {
        // Arrange & Act
        var element = new ObjectElement();

        // Assert
        Assert.AreEqual("object", ObjectElement.ElementName);
        Assert.AreEqual('{', element.Delimiter.OpeningSpecifier);
        Assert.AreEqual('}', element.Delimiter.ClosingSpecifier);
        Assert.AreEqual(ElementStyle.Compact, element.Delimiter.Style);
        Assert.AreEqual(1, element.Delimiter.SpecifierCount);
        Assert.AreEqual(0, element.Count);
        Assert.AreEqual(0, element.Children.Count);
    }

    #endregion

    #region Inheritance Tests

    [TestMethod]
    public void Inheritance_IsDictionaryElement()
    {
        // Arrange & Act
        var element = new ObjectElement();

        // Assert
        Assert.IsInstanceOfType(element, typeof(DictionaryElement));
        Assert.IsInstanceOfType(element, typeof(CollectionElement));
        Assert.IsInstanceOfType(element, typeof(Element));
    }

    [TestMethod]
    public void Inheritance_HasDictionaryElementProperties()
    {
        // Arrange
        var element = new ObjectElement();

        // Act & Assert
        Assert.AreEqual(0, element.Count);
        Assert.IsNotNull(element.Values);
        Assert.IsNotNull(element.Children);
    }

    #endregion

    #region Key-Value Pair Management Tests

    [TestMethod]
    public void AddOrUpdate_NewKeyValuePair_AddsSuccessfully()
    {
        // Arrange
        var obj = new ObjectElement();
        var kvp = new KeyValuePairElement(new KeywordElement("name"), new StringElement("John"));

        // Act
        obj.AddOrUpdate(kvp);

        // Assert
        Assert.AreEqual(1, obj.Count);
        Assert.IsTrue(obj.ContainsKey("name"));
        Assert.AreEqual("John", ((StringElement)obj.GetElement("name")).Value);
        Assert.AreEqual(kvp, obj.Children.First());
    }

    [TestMethod]
    public void AddOrUpdate_ExistingKey_UpdatesValue()
    {
        // Arrange
        var obj = new ObjectElement();
        var kvp1 = new KeyValuePairElement(new KeywordElement("name"), new StringElement("John"));
        var kvp2 = new KeyValuePairElement(new KeywordElement("name"), new StringElement("Jane"));

        // Act
        obj.AddOrUpdate(kvp1);
        obj.AddOrUpdate(kvp2);

        // Assert
        Assert.AreEqual(1, obj.Count);
        Assert.IsTrue(obj.ContainsKey("name"));
        Assert.AreEqual("Jane", ((StringElement)obj.GetElement("name")).Value);
        Assert.AreEqual(1, obj.Children.Count);
    }

    [TestMethod]
    public void IndexerSet_NewKey_AddsKeyValuePair()
    {
        // Arrange
        var obj = new ObjectElement();
        var value = new StringElement("test");

        // Act
        obj["testKey"] = value;

        // Assert
        Assert.AreEqual(1, obj.Count);
        Assert.IsTrue(obj.ContainsKey("testKey"));
        Assert.AreEqual(value, obj.GetElement("testKey"));
    }

    [TestMethod]
    public void IndexerSet_ExistingKey_UpdatesValue()
    {
        // Arrange
        var obj = new ObjectElement();
        var value1 = new StringElement("test1");
        var value2 = new StringElement("test2");

        // Act
        obj["testKey"] = value1;
        obj["testKey"] = value2;

        // Assert
        Assert.AreEqual(1, obj.Count);
        Assert.AreEqual(value2, obj.GetElement("testKey"));
    }

    [TestMethod]
    public void IndexerGet_ExistingKey_ReturnsValue()
    {
        // Arrange
        var obj = new ObjectElement();
        var value = new StringElement("test");
        obj["testKey"] = value;

        // Act
        var result = obj["testKey"];

        // Assert
        Assert.AreEqual(value, result);
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void IndexerGet_NonExistentKey_ThrowsException()
    {
        // Arrange
        var obj = new ObjectElement();

        // Act
        var result = obj["nonExistentKey"];

        // Assert - Exception expected
    }

    #endregion

    #region Key Operations Tests

    [TestMethod]
    public void ContainsKey_ExistingKey_ReturnsTrue()
    {
        // Arrange
        var obj = new ObjectElement();
        obj["testKey"] = new StringElement("test");

        // Act
        var result = obj.ContainsKey("testKey");

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ContainsKey_NonExistentKey_ReturnsFalse()
    {
        // Arrange
        var obj = new ObjectElement();

        // Act
        var result = obj.ContainsKey("nonExistentKey");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ContainsKey_CaseSensitive_ReturnsCorrectResult()
    {
        // Arrange
        var obj = new ObjectElement();
        obj["testKey"] = new StringElement("test");

        // Act & Assert
        Assert.IsTrue(obj.ContainsKey("testKey"));
        Assert.IsFalse(obj.ContainsKey("TestKey"));
        Assert.IsFalse(obj.ContainsKey("TESTKEY"));
    }

    [TestMethod]
    public void GetElement_ExistingKey_ReturnsElement()
    {
        // Arrange
        var obj = new ObjectElement();
        var value = new StringElement("test");
        obj["testKey"] = value;

        // Act
        var result = obj.GetElement("testKey");

        // Assert
        Assert.AreEqual(value, result);
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void GetElement_NonExistentKey_ThrowsException()
    {
        // Arrange
        var obj = new ObjectElement();

        // Act
        obj.GetElement("nonExistentKey");

        // Assert - Exception expected
    }

    [TestMethod]
    public void TryGetElement_ExistingKey_ReturnsTrue()
    {
        // Arrange
        var obj = new ObjectElement();
        var value = new StringElement("test");
        obj["testKey"] = value;

        // Act
        var result = obj.TryGetElement<StringElement>("testKey", out var element);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(value, element);
    }

    [TestMethod]
    public void TryGetElement_NonExistentKey_ReturnsFalse()
    {
        // Arrange
        var obj = new ObjectElement();

        // Act
        var result = obj.TryGetElement<StringElement>("nonExistentKey", out var element);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(element);
    }

    [TestMethod]
    public void TryGetElement_WrongType_ReturnsFalse()
    {
        // Arrange
        var obj = new ObjectElement();
        obj["testKey"] = new StringElement("test");

        // Act
        var result = obj.TryGetElement<IntegerElement>("testKey", out var element);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(element);
    }

    #endregion

    #region Remove Operations Tests

    [TestMethod]
    public void Remove_ExistingKey_RemovesAndReturnsTrue()
    {
        // Arrange
        var obj = new ObjectElement();
        obj["testKey"] = new StringElement("test");

        // Act
        var result = obj.Remove("testKey");

        // Assert
        Assert.IsTrue(result);
        Assert.IsFalse(obj.ContainsKey("testKey"));
        Assert.AreEqual(0, obj.Count);
        Assert.AreEqual(0, obj.Children.Count);
    }

    [TestMethod]
    public void Remove_NonExistentKey_ReturnsFalse()
    {
        // Arrange
        var obj = new ObjectElement();

        // Act
        var result = obj.Remove("nonExistentKey");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void RemoveChild_KeyValuePairElement_RemovesFromBoth()
    {
        // Arrange
        var obj = new ObjectElement();
        var kvp = new KeyValuePairElement(new KeywordElement("testKey"), new StringElement("test"));
        obj.AddOrUpdate(kvp);

        // Act
        var result = obj.RemoveChild(kvp);

        // Assert
        Assert.IsTrue(result);
        Assert.IsFalse(obj.ContainsKey("testKey"));
        Assert.AreEqual(0, obj.Count);
        Assert.AreEqual(0, obj.Children.Count);
    }

    [TestMethod]
    public void RemoveAllChildren_WithMultipleElements_RemovesAll()
    {
        // Arrange
        var obj = new ObjectElement();
        obj["key1"] = new StringElement("value1");
        obj["key2"] = new StringElement("value2");
        obj["key3"] = new StringElement("value3");

        // Act
        var removedCount = obj.RemoveAllChildren();

        // Assert
        Assert.AreEqual(3, removedCount);
        Assert.AreEqual(0, obj.Count);
        Assert.AreEqual(0, obj.Children.Count);
    }

    #endregion

    #region ProcessingInstruction Tests

    [TestMethod]
    public void AddOrUpdate_ProcessingInstruction_AddsToChildrenOnly()
    {
        // Arrange
        var obj = new ObjectElement();
        var pi = new IdProcessingInstruction(new StringElement("testId"));

        // Act
        obj.AddOrUpdate(pi);

        // Assert
        Assert.AreEqual(0, obj.Count); // Not added to dictionary
        Assert.AreEqual(1, obj.Children.Count); // Added to children
        Assert.AreEqual(pi, obj.Children.First());
        Assert.AreEqual(obj, pi.Parent);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void AddOrUpdate_UnsupportedElement_ThrowsException()
    {
        // Arrange
        var obj = new ObjectElement();
        var unsupportedElement = new StringElement("test");

        // Act
        obj.AddOrUpdate(unsupportedElement);

        // Assert - Exception expected
    }

    #endregion

    #region Serialization Tests - Default Formatting

    [TestMethod]
    public void ToXfer_EmptyObject_ReturnsCorrectFormat()
    {
        // Arrange
        var obj = new ObjectElement();

        // Act
        var result = obj.ToXfer();

        // Assert
        Assert.AreEqual("{}", result);
    }

    [TestMethod]
    public void ToXfer_SingleKeyValue_ReturnsCorrectFormat()
    {
        // Arrange
        var obj = new ObjectElement();
        obj["name"] = new StringElement("John");

        // Act
        var result = obj.ToXfer();

        // Assert
        Assert.IsTrue(result.StartsWith("{"));
        Assert.IsTrue(result.EndsWith("}"));
        Assert.IsTrue(result.Contains("name \"John\""));
    }

    [TestMethod]
    public void ToXfer_MultipleKeyValues_ReturnsCorrectFormat()
    {
        // Arrange
        var obj = new ObjectElement();
        obj["name"] = new StringElement("John");
        obj["age"] = new IntegerElement(30);

        // Act
        var result = obj.ToXfer();

        // Assert - canonical format is compact without space after opening brace
        Assert.IsTrue(result.StartsWith("{"));
        Assert.IsTrue(result.EndsWith(" }"));
        Assert.IsTrue(result.Contains("name \"John\""));
        Assert.IsTrue(result.Contains("age #30"));
    }

    [TestMethod]
    public void ToXfer_ExplicitStyle_ReturnsCorrectFormat()
    {
        // Arrange
        var obj = new ObjectElement();
        obj.Delimiter.Style = ElementStyle.Explicit;
        obj["name"] = new StringElement("John");

        // Act
        var result = obj.ToXfer();

        // Assert - explicit style uses <{ and }> delimiters without extra spaces
        Assert.IsTrue(result.StartsWith("<{"));
        Assert.IsTrue(result.EndsWith("}>"));
        Assert.IsTrue(result.Contains("name \"John\""));
    }

    #endregion

    #region Serialization Tests - Formatted

    [TestMethod]
    public void ToXfer_WithIndentation_ReturnsFormattedOutput()
    {
        // Arrange
        var obj = new ObjectElement();
        obj["name"] = new StringElement("John");
        obj["age"] = new IntegerElement(30);

        // Act
        var result = obj.ToXfer(Formatting.Indented);

        // Assert
        Assert.IsTrue(result.Contains("{\r\n"));
        Assert.IsTrue(result.Contains("  name \"John\"\r\n"));
        Assert.IsTrue(result.Contains("  age #30 \r\n"));
        Assert.IsTrue(result.EndsWith("}"));
    }

    [TestMethod]
    public void ToXfer_WithCustomIndentation_ReturnsFormattedOutput()
    {
        // Arrange
        var obj = new ObjectElement();
        obj["name"] = new StringElement("John");

        // Act
        var result = obj.ToXfer(Formatting.Indented, indentChar: '\t', indentation: 1);

        // Assert
        Assert.IsTrue(result.Contains("{\r\n"));
        Assert.IsTrue(result.Contains("\tname \"John\"\r\n"));
        Assert.IsTrue(result.EndsWith("}"));
    }

    #endregion

    #region Nested Objects Tests

    [TestMethod]
    public void NestedObjects_SimpleNesting_WorksCorrectly()
    {
        // Arrange
        var innerObj = new ObjectElement();
        innerObj["city"] = new StringElement("New York");

        var outerObj = new ObjectElement();
        outerObj["name"] = new StringElement("John");
        outerObj["address"] = innerObj;

        // Act
        var result = outerObj.ToXfer();

        // Assert
        Assert.IsTrue(result.Contains("name \"John\""));
        Assert.IsTrue(result.Contains("address { "));
        Assert.IsTrue(result.Contains("city \"New York\""));
    }

    [TestMethod]
    public void NestedObjects_MultipleNesting_WorksCorrectly()
    {
        // Arrange
        var level3 = new ObjectElement();
        level3["value"] = new StringElement("deep");

        var level2 = new ObjectElement();
        level2["nested"] = level3;

        var level1 = new ObjectElement();
        level1["child"] = level2;

        // Act
        var result = level1.ToXfer();

        // Assert
        Assert.IsTrue(result.Contains("child { "));
        Assert.IsTrue(result.Contains("nested { "));
        Assert.IsTrue(result.Contains("value \"deep\""));
    }

    #endregion

    #region Collection Property Tests

    [TestMethod]
    public void Dictionary_Property_ReturnsReadOnlyView()
    {
        // Arrange
        var obj = new ObjectElement();
        obj["key1"] = new StringElement("value1");
        obj["key2"] = new StringElement("value2");

        // Act
        var dictionary = obj.Dictionary;

        // Assert
        Assert.AreEqual(2, dictionary.Count);
        Assert.IsTrue(dictionary.ContainsKey("key1"));
        Assert.IsTrue(dictionary.ContainsKey("key2"));
    }

    [TestMethod]
    public void TypedValue_Property_ReturnsKeyValuePairs()
    {
        // Arrange
        var obj = new ObjectElement();
        obj["key1"] = new StringElement("value1");
        obj["key2"] = new StringElement("value2");

        // Act
        var typedValue = obj.TypedValue;

        // Assert
        Assert.AreEqual(2, typedValue.Count);
        Assert.IsTrue(typedValue.Any(kvp => kvp.Key == "key1"));
        Assert.IsTrue(typedValue.Any(kvp => kvp.Key == "key2"));
    }

    [TestMethod]
    public void Values_Property_ReturnsEnumerable()
    {
        // Arrange
        var obj = new ObjectElement();
        obj["key1"] = new StringElement("value1");
        obj["key2"] = new StringElement("value2");

        // Act
        var values = obj.Values.ToList();

        // Assert
        Assert.AreEqual(2, values.Count);
        Assert.IsTrue(values.All(v => v is KeyValuePairElement));
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void EdgeCases_EmptyKeys_HandleCorrectly()
    {
        // Arrange
        var obj = new ObjectElement();

        // Act
        obj[""] = new StringElement("empty key");

        // Assert
        Assert.AreEqual(1, obj.Count);
        Assert.IsTrue(obj.ContainsKey(""));
        Assert.AreEqual("empty key", ((StringElement)obj.GetElement("")).Value);
    }

    [TestMethod]
    public void EdgeCases_SpecialCharacterKeys_HandleCorrectly()
    {
        // Arrange
        var obj = new ObjectElement();
        var specialKeys = new[] { "key with spaces", "key-with-dashes", "key_with_underscores", "key.with.dots", "key@with@symbols" };

        // Act
        foreach (var key in specialKeys)
        {
            obj[key] = new StringElement($"value for {key}");
        }

        // Assert
        Assert.AreEqual(specialKeys.Length, obj.Count);
        foreach (var key in specialKeys)
        {
            Assert.IsTrue(obj.ContainsKey(key));
        }
    }

    [TestMethod]
    public void EdgeCases_MixedValueTypes_HandleCorrectly()
    {
        // Arrange
        var obj = new ObjectElement();

        // Act
        obj["string"] = new StringElement("text");
        obj["integer"] = new IntegerElement(42);
        obj["long"] = new LongElement(1000000L);
        obj["double"] = new DoubleElement(3.14);
        obj["bool"] = new BooleanElement(true);

        // Assert
        Assert.AreEqual(5, obj.Count);
        Assert.IsInstanceOfType(obj.GetElement("string"), typeof(StringElement));
        Assert.IsInstanceOfType(obj.GetElement("integer"), typeof(IntegerElement));
        Assert.IsInstanceOfType(obj.GetElement("long"), typeof(LongElement));
        Assert.IsInstanceOfType(obj.GetElement("double"), typeof(DoubleElement));
        Assert.IsInstanceOfType(obj.GetElement("bool"), typeof(BooleanElement));
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_ReturnsXferRepresentation()
    {
        // Arrange
        var obj = new ObjectElement();
        obj["name"] = new StringElement("John");

        // Act
        var result = obj.ToString();

        // Assert
        Assert.IsTrue(result.Contains("name \"John\""));
        Assert.IsTrue(result.StartsWith("{ "));
        Assert.IsTrue(result.EndsWith(" }"));
    }

    [TestMethod]
    public void ToString_EmptyObject_ReturnsEmptyFormat()
    {
        // Arrange
        var obj = new ObjectElement();

        // Act
        var result = obj.ToString();

        // Assert
        Assert.AreEqual("{}", result);
    }

    #endregion

    #region Realistic Use Cases

    [TestMethod]
    public void RealisticUseCases_PersonObject_WorksCorrectly()
    {
        // Arrange
        var person = new ObjectElement();
        person["name"] = new StringElement("John Doe");
        person["age"] = new IntegerElement(30);
        person["email"] = new StringElement("john.doe@example.com");

        // Act
        var xfer = person.ToXfer(Formatting.Indented);

        // Assert
        Assert.IsTrue(xfer.Contains("name \"John Doe\""));
        Assert.IsTrue(xfer.Contains("age #30"));
        Assert.IsTrue(xfer.Contains("email \"john.doe@example.com\""));
    }

    [TestMethod]
    public void RealisticUseCases_ConfigurationObject_WorksCorrectly()
    {
        // Arrange
        var config = new ObjectElement();
        config["server"] = new StringElement("localhost");
        config["port"] = new IntegerElement(8080);
        config["ssl"] = new BooleanElement(true);
        config["timeout"] = new LongElement(30000L);

        // Act
        var xfer = config.ToXfer();

        // Assert
        Assert.IsTrue(xfer.Contains("server \"localhost\""));
        Assert.IsTrue(xfer.Contains("port #8080"));
        Assert.IsTrue(xfer.Contains("ssl true"));
        Assert.IsTrue(xfer.Contains("timeout &30000"));
    }

    #endregion
}
