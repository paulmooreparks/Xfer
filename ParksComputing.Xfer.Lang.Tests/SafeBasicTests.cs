using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;
using System.Text;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class SafeBasicTests
{
    [TestMethod]
    public void Parse_EmptyObject_ShouldSucceed()
    {
        // Arrange
        var parser = new Parser();
        var input = "{}";
        var bytes = Encoding.UTF8.GetBytes(input);

        // Act
        var document = parser.Parse(bytes);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);
        Assert.IsInstanceOfType<ObjectElement>(document.Root);
        Assert.IsFalse(document.HasError);
    }

    [TestMethod]
    public void Parse_SimpleString_ShouldSucceed()
    {
        // Arrange
        var parser = new Parser();
        var input = """{ name "Alice" }""";
        var bytes = Encoding.UTF8.GetBytes(input);

        // Act
        var document = parser.Parse(bytes);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsInstanceOfType<ObjectElement>(document.Root);
        var obj = (ObjectElement)document.Root;
        Assert.IsTrue(obj.Dictionary.ContainsKey("name"));
        var nameElement = obj["name"] as StringElement;
        Assert.IsNotNull(nameElement);
        Assert.AreEqual("Alice", nameElement.Value);
    }

    [TestMethod]
    public void Parse_SimpleInteger_ShouldSucceed()
    {
        // Arrange
        var parser = new Parser();
        var input = """{ age 42 }""";
        var bytes = Encoding.UTF8.GetBytes(input);

        // Act
        var document = parser.Parse(bytes);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsInstanceOfType<ObjectElement>(document.Root);
        var obj = (ObjectElement)document.Root;
        Assert.IsTrue(obj.Dictionary.ContainsKey("age"));
        var ageElement = obj["age"] as IntegerElement;
        Assert.IsNotNull(ageElement);
        Assert.AreEqual(42, ageElement.Value);
    }

    [TestMethod]
    public void Parse_SimpleBoolean_ShouldSucceed()
    {
        // Arrange
        var parser = new Parser();
        var input = """{ active ~true }""";
        var bytes = Encoding.UTF8.GetBytes(input);

        // Act
        var document = parser.Parse(bytes);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsInstanceOfType<ObjectElement>(document.Root);
        var obj = (ObjectElement)document.Root;
        Assert.IsTrue(obj.Dictionary.ContainsKey("active"));
        var activeElement = obj["active"] as BooleanElement;
        Assert.IsNotNull(activeElement);
        Assert.AreEqual(true, activeElement.Value);
    }

    [TestMethod]
    public void Parse_SimpleDecimal_ShouldSucceed()
    {
        // Arrange
        var parser = new Parser();
        var input = """{ price *99.99 }""";
        var bytes = Encoding.UTF8.GetBytes(input);

        // Act
        var document = parser.Parse(bytes);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsInstanceOfType<ObjectElement>(document.Root);
        var obj = (ObjectElement)document.Root;
        Assert.IsTrue(obj.Dictionary.ContainsKey("price"));
        var priceElement = obj["price"] as DecimalElement;
        Assert.IsNotNull(priceElement);
        Assert.AreEqual(99.99m, priceElement.Value);
    }
}
