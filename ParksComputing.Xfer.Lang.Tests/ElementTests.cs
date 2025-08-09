using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class ElementTests
{
    [TestMethod]
    public void StringElement_Constructor_SetsValue()
    {
        // Arrange & Act
        var element = new StringElement("Hello World");

        // Assert
        Assert.AreEqual("Hello World", element.Value);
    }

    [TestMethod]
    public void StringElement_ToXfer_ReturnsQuotedString()
    {
        // Arrange
        var element = new StringElement("Hello World");

        // Act
        var xfer = element.ToXfer();

        // Assert
        Assert.AreEqual("\"Hello World\"", xfer);
    }

    [TestMethod]
    public void IntegerElement_Constructor_SetsValue()
    {
        // Arrange & Act
        var element = new IntegerElement(42);

        // Assert
        Assert.AreEqual(42, element.Value);
    }

    [TestMethod]
    public void DoubleElement_Constructor_SetsValue()
    {
        // Arrange & Act
        var element = new DoubleElement(3.14);

        // Assert
        Assert.AreEqual(3.14, element.Value);
    }

    [TestMethod]
    public void BooleanElement_Constructor_SetsValue()
    {
        // Arrange & Act
        var element = new BooleanElement(true);

        // Assert
        Assert.AreEqual(true, element.Value);
    }

    [TestMethod]
    public void BooleanElement_ToXfer_ReturnsCorrectValue()
    {
        // Arrange
        var trueElement = new BooleanElement(true);
        var falseElement = new BooleanElement(false);

        // Act & Assert
        Assert.AreEqual("~true ", trueElement.ToXfer());
        Assert.AreEqual("~false ", falseElement.ToXfer());
    }

    [TestMethod]
    public void ObjectElement_Constructor_CreatesEmptyObject()
    {
        // Arrange & Act
        var element = new ObjectElement();

        // Assert
        Assert.IsNotNull(element.Dictionary);
        Assert.AreEqual(0, element.Dictionary.Count);
    }

    [TestMethod]
    public void ArrayElement_Constructor_CreatesEmptyArray()
    {
        // Arrange & Act
        var element = new ArrayElement();

        // Assert
        Assert.AreEqual(0, element.Count);
    }

    [TestMethod]
    public void ArrayElement_Add_AddsElement()
    {
        // Arrange
        var array = new ArrayElement();
        var stringElement = new StringElement("test");

        // Act
        array.Add(stringElement);

        // Assert
        Assert.AreEqual(1, array.Count);
        Assert.AreEqual(stringElement, array[0]);
    }

    [TestMethod]
    public void ArrayElement_HomogeneousTypeEnforcement_WorksCorrectly()
    {
        // Arrange
        var array = new ArrayElement();

        // Act
        array.Add(new IntegerElement(1));
        array.Add(new IntegerElement(2));
        array.Add(new IntegerElement(3));

        // Assert
        Assert.AreEqual(3, array.Count);
        Assert.AreEqual(typeof(IntegerElement), array.ElementType);
    }

    [TestMethod]
    public void ArrayElement_MixedTypes_ThrowsException()
    {
        // Arrange
        var array = new ArrayElement();
        array.Add(new IntegerElement(1));

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            array.Add(new StringElement("test"));
        });
    }

    [TestMethod]
    public void KeyValuePairElement_Constructor_SetsKeyAndValue()
    {
        // Arrange
        var key = new KeywordElement("name");
        var value = new StringElement("Alice");

        // Act
        var kvp = new KeyValuePairElement(key);
        kvp.Value = value;

        // Assert
        Assert.AreEqual(key, kvp.KeyElement);
        Assert.AreEqual(value, kvp.Value);
        Assert.AreEqual("name", kvp.Key);
    }

    [TestMethod]
    public void ObjectElement_Add_AddsKeyValuePair()
    {
        // Arrange
        var obj = new ObjectElement();
        var key = new KeywordElement("name");
        var value = new StringElement("test");
        var kvp = new KeyValuePairElement(key);
        kvp.Value = value;

        // Act
        obj.Add(kvp);

        // Assert
        Assert.AreEqual(1, obj.Dictionary.Count);
        Assert.IsTrue(obj.Dictionary.ContainsKey("name"));
    }

    [TestMethod]
    public void CharacterElement_Constructor_SetsValue()
    {
        // Arrange & Act
        var element = new CharacterElement('A');

        // Assert
        Assert.AreEqual('A', element.Value);
    }
}
