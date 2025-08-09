using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.ProcessingInstructions;
using System;
using System.Linq;

namespace ParksComputing.Xfer.Lang.Tests;

/// <summary>
/// Comprehensive tests for DictionaryElement functionality.
/// Tests key-value pair management, collection operations, and semantic vs non-semantic element handling.
/// Uses ObjectElement as a concrete implementation for testing.
/// </summary>
[TestClass]
public class DictionaryElementTests
{
    #region Helper Methods

    private ObjectElement CreateTestDictionary()
    {
        return new ObjectElement();
    }

    private KeyValuePairElement CreateKvp(string key, string value)
    {
        return new KeyValuePairElement(new KeywordElement(key), new StringElement(value));
    }

    private KeyValuePairElement CreateKvp(string key, Element value)
    {
        return new KeyValuePairElement(new KeywordElement(key), value);
    }

    #endregion

    #region Constructor and Basic Properties

    [TestMethod]
    public void Constructor_InitializesEmptyDictionary()
    {
        // Arrange & Act
        var dict = CreateTestDictionary();

        // Assert
        Assert.AreEqual(0, dict.Count);
        Assert.AreEqual(0, dict.Values.Count());
        Assert.AreEqual(0, dict.Children.Count);
    }

    [TestMethod]
    public void Count_ReflectsSemanticElementsOnly()
    {
        // Arrange
        var dict = CreateTestDictionary();
        var kvp1 = CreateKvp("key1", "value1");
        var kvp2 = CreateKvp("key2", "value2");
        var comment = new CommentElement();

        // Act
        dict.Add(kvp1);
        dict.Add(kvp2);
        dict.Add(comment);

        // Assert
        Assert.AreEqual(2, dict.Count); // Only KVPs count
        Assert.AreEqual(3, dict.Children.Count); // All children count
    }

    #endregion

    #region Add Method Tests

    [TestMethod]
    public void Add_KeyValuePair_AddsSuccessfully()
    {
        // Arrange
        var dict = CreateTestDictionary();
        var kvp = CreateKvp("testKey", "testValue");

        // Act
        bool result = dict.Add(kvp);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(1, dict.Count);
        Assert.IsTrue(dict.Values.Contains(kvp));
        Assert.IsTrue(dict.Children.Contains(kvp));
        Assert.AreEqual(dict, kvp.Parent);
    }

    [TestMethod]
    public void Add_DuplicateKey_ReturnsFalse()
    {
        // Arrange
        var dict = CreateTestDictionary();
        var kvp1 = CreateKvp("sameKey", "value1");
        var kvp2 = CreateKvp("sameKey", "value2");

        // Act
        bool result1 = dict.Add(kvp1);
        bool result2 = dict.Add(kvp2);

        // Assert
        Assert.IsTrue(result1);
        Assert.IsFalse(result2);
        Assert.AreEqual(1, dict.Count);
    }

    [TestMethod]
    public void Add_Comment_AddsToChildrenOnly()
    {
        // Arrange
        var dict = CreateTestDictionary();
        var comment = new CommentElement();

        // Act
        bool result = dict.Add(comment);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(0, dict.Count); // Doesn't count as semantic element
        Assert.AreEqual(1, dict.Children.Count);
        Assert.IsTrue(dict.Children.Contains(comment));
        Assert.AreEqual(dict, comment.Parent);
    }

    [TestMethod]
    public void Add_ProcessingInstruction_AddsToChildrenOnly()
    {
        // Arrange
        var dict = CreateTestDictionary();
        var pi = new ProcessingInstruction(new StringElement("value"), "test");

        // Act
        bool result = dict.Add(pi);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(0, dict.Count); // Doesn't count as semantic element
        Assert.AreEqual(1, dict.Children.Count);
        Assert.IsTrue(dict.Children.Contains(pi));
        Assert.AreEqual(dict, pi.Parent);
    }

    [TestMethod]
    public void Add_OtherElementType_ReturnsFalse()
    {
        // Arrange
        var dict = CreateTestDictionary();
        var stringElement = new StringElement("not a kvp or comment");

        // Act
        bool result = dict.Add(stringElement);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(0, dict.Count);
        Assert.AreEqual(0, dict.Children.Count);
    }

    [TestMethod]
    public void Add_SameElementTwice_DoesNotDuplicate()
    {
        // Arrange
        var dict = CreateTestDictionary();
        var kvp = CreateKvp("key", "value");

        // Act
        dict.Add(kvp);
        dict.Add(kvp); // Same instance

        // Assert
        Assert.AreEqual(1, dict.Count);
        Assert.AreEqual(1, dict.Children.Count);
    }

    #endregion

    #region Add with Key Method Tests

    [TestMethod]
    public void AddWithKey_NewKey_AddsSuccessfully()
    {
        // Arrange
        var dict = CreateTestDictionary();
        var kvp = CreateKvp("testKey", "testValue");

        // Act
        dict.Add("testKey", kvp);

        // Assert
        Assert.AreEqual(1, dict.Count);
        Assert.IsTrue(dict.Values.Contains(kvp));
    }

    [TestMethod]
    public void AddWithKey_ExistingKey_ReplacesValue()
    {
        // Arrange
        var dict = CreateTestDictionary();
        var kvp1 = CreateKvp("key", "value1");
        var kvp2 = CreateKvp("key", "value2");

        // Act
        dict.Add("key", kvp1);
        dict.Add("key", kvp2); // Should replace

        // Assert
        Assert.AreEqual(1, dict.Count);
        Assert.IsTrue(dict.Values.Contains(kvp2));
        Assert.IsFalse(dict.Values.Contains(kvp1));
    }

    #endregion

    #region GetValue Method Tests

    [TestMethod]
    public void GetValue_ExistingKey_ReturnsValue()
    {
        // Arrange
        var dict = CreateTestDictionary();
        var kvp = CreateKvp("testKey", "testValue");
        dict.Add(kvp);

        // Act
        var result = dict.GetValue("testKey");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(kvp, result);
    }

    [TestMethod]
    public void GetValue_NonExistentKey_ReturnsNull()
    {
        // Arrange
        var dict = CreateTestDictionary();

        // Act
        var result = dict.GetValue("nonExistentKey");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetValue_CaseSensitive_ReturnsCorrectValue()
    {
        // Arrange
        var dict = CreateTestDictionary();
        var kvp1 = CreateKvp("Key", "value1");
        var kvp2 = CreateKvp("key", "value2");
        dict.Add(kvp1);
        dict.Add(kvp2);

        // Act
        var result1 = dict.GetValue("Key");
        var result2 = dict.GetValue("key");

        // Assert
        Assert.AreEqual(kvp1, result1);
        Assert.AreEqual(kvp2, result2);
    }

    #endregion

    #region GetElementAt Method Tests

    [TestMethod]
    public void GetElementAt_ValidIndex_ReturnsElement()
    {
        // Arrange
        var dict = CreateTestDictionary();
        var kvp1 = CreateKvp("key1", "value1");
        var kvp2 = CreateKvp("key2", "value2");
        dict.Add(kvp1);
        dict.Add(kvp2);

        // Act
        var result0 = dict.GetElementAt(0);
        var result1 = dict.GetElementAt(1);

        // Assert
        Assert.IsNotNull(result0);
        Assert.IsNotNull(result1);
        Assert.IsTrue(result0 == kvp1 || result0 == kvp2);
        Assert.IsTrue(result1 == kvp1 || result1 == kvp2);
        Assert.AreNotEqual(result0, result1);
    }

    [TestMethod]
    public void GetElementAt_InvalidIndex_ReturnsNull()
    {
        // Arrange
        var dict = CreateTestDictionary();
        var kvp = CreateKvp("key", "value");
        dict.Add(kvp);

        // Act
        var resultNegative = dict.GetElementAt(-1);
        var resultTooHigh = dict.GetElementAt(1);

        // Assert
        Assert.IsNull(resultNegative);
        Assert.IsNull(resultTooHigh);
    }

    [TestMethod]
    public void GetElementAt_EmptyDictionary_ReturnsNull()
    {
        // Arrange
        var dict = CreateTestDictionary();

        // Act
        var result = dict.GetElementAt(0);

        // Assert
        Assert.IsNull(result);
    }

    #endregion

    #region Values Property Tests

    [TestMethod]
    public void Values_ReturnsAllKeyValuePairs()
    {
        // Arrange
        var dict = CreateTestDictionary();
        var kvp1 = CreateKvp("key1", "value1");
        var kvp2 = CreateKvp("key2", "value2");
        var comment = new CommentElement();

        dict.Add(kvp1);
        dict.Add(kvp2);
        dict.Add(comment);

        // Act
        var values = dict.Values.ToList();

        // Assert
        Assert.AreEqual(2, values.Count);
        Assert.IsTrue(values.Contains(kvp1));
        Assert.IsTrue(values.Contains(kvp2));
        Assert.IsFalse(values.Contains(comment));
    }

    [TestMethod]
    public void Values_EmptyDictionary_ReturnsEmpty()
    {
        // Arrange
        var dict = CreateTestDictionary();

        // Act
        var values = dict.Values.ToList();

        // Assert
        Assert.AreEqual(0, values.Count);
    }

    #endregion

    #region ToString Method Tests

    [TestMethod]
    public void ToString_WithKeyValuePairs_ReturnsSerializedString()
    {
        // Arrange
        var dict = CreateTestDictionary();
        var kvp1 = CreateKvp("key1", "value1");
        var kvp2 = CreateKvp("key2", "value2");
        dict.Add(kvp1);
        dict.Add(kvp2);

        // Act
        var result = dict.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("key1"));
        Assert.IsTrue(result.Contains("key2"));
    }

    [TestMethod]
    public void ToString_EmptyDictionary_ReturnsEmptyString()
    {
        // Arrange
        var dict = CreateTestDictionary();

        // Act
        var result = dict.ToString();

        // Assert
        Assert.AreEqual("{}", result);
    }

    #endregion

    #region Complex Scenarios

    [TestMethod]
    public void ComplexScenario_MixedElements_HandledCorrectly()
    {
        // Arrange
        var dict = CreateTestDictionary();
        var kvp1 = CreateKvp("name", "John");
        var kvp2 = CreateKvp("age", new IntegerElement(30));
        var kvp3 = CreateKvp("active", new BooleanElement(true));
        var comment = new CommentElement();
        var pi = new ProcessingInstruction(new StringElement("debug"), "mode");

        // Act
        dict.Add(kvp1);
        dict.Add(comment);
        dict.Add(kvp2);
        dict.Add(pi);
        dict.Add(kvp3);

        // Assert
        Assert.AreEqual(3, dict.Count); // Only KVPs
        Assert.AreEqual(5, dict.Children.Count); // All elements

        // Verify semantic elements
        Assert.IsNotNull(dict.GetValue("name"));
        Assert.IsNotNull(dict.GetValue("age"));
        Assert.IsNotNull(dict.GetValue("active"));
        Assert.IsNull(dict.GetValue("mode")); // PI not in semantic dictionary

        // Verify all elements have correct parent
        Assert.AreEqual(dict, kvp1.Parent);
        Assert.AreEqual(dict, comment.Parent);
        Assert.AreEqual(dict, pi.Parent);
    }

    [TestMethod]
    public void ComplexScenario_LargeNumberOfElements_PerformsWell()
    {
        // Arrange
        var dict = CreateTestDictionary();

        // Act - Add many elements
        for (int i = 0; i < 1000; i++)
        {
            var kvp = CreateKvp($"key{i}", $"value{i}");
            dict.Add(kvp);
        }

        // Assert
        Assert.AreEqual(1000, dict.Count);
        Assert.AreEqual(1000, dict.Children.Count);

        // Verify random access
        Assert.IsNotNull(dict.GetValue("key500"));
        Assert.IsNotNull(dict.GetElementAt(500));
    }

    [TestMethod]
    public void ComplexScenario_NestedElements_HandledCorrectly()
    {
        // Arrange
        var dict = CreateTestDictionary();
        var nestedObject = new ObjectElement();
        nestedObject["innerKey"] = new StringElement("innerValue");

        var kvp = CreateKvp("nested", nestedObject);

        // Act
        dict.Add(kvp);

        // Assert
        Assert.AreEqual(1, dict.Count);
        var retrieved = dict.GetValue("nested") as KeyValuePairElement;
        Assert.IsNotNull(retrieved);
        Assert.IsInstanceOfType(retrieved.Value, typeof(ObjectElement));
    }

    #endregion

    #region Parent Relationship Tests

    [TestMethod]
    public void ParentRelationships_SetCorrectlyOnAdd()
    {
        // Arrange
        var dict = CreateTestDictionary();
        var kvp = CreateKvp("key", "value");
        var comment = new CommentElement();

        // Act
        dict.Add(kvp);
        dict.Add(comment);

        // Assert
        Assert.AreEqual(dict, kvp.Parent);
        Assert.AreEqual(dict, comment.Parent);
    }

    [TestMethod]
    public void ParentRelationships_NotSetOnFailedAdd()
    {
        // Arrange
        var dict = CreateTestDictionary();
        var stringElement = new StringElement("not addable");

        // Act
        dict.Add(stringElement);

        // Assert
        Assert.IsNull(stringElement.Parent);
    }

    #endregion
}
