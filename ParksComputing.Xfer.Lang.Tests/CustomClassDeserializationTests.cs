using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class CustomClassDeserializationTests
{
    public class SimpleTestClass
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    [TestMethod]
    public void Deserialize_CustomClass_ShouldWork()
    {
        // Arrange
        var original = new SimpleTestClass
        {
            Name = "Test User",
            Age = 25,
            IsActive = true
        };

        // Serialize first to see what XferLang content is generated
        var xferContent = XferConvert.Serialize(original, Formatting.Pretty);
        Console.WriteLine("Generated XferLang content:");
        Console.WriteLine(xferContent);

        // Parse to see what document structure we get
        var document = XferParser.Parse(xferContent);
        Console.WriteLine($"Document Root Type: {document.Root.GetType().Name}");
        Console.WriteLine($"Document Root Values Count: {document.Root.Values.Count()}");

        if (document.Root.Values.Any())
        {
            var first = document.Root.Values.First();
            Console.WriteLine($"First Value Type: {first.GetType().Name}");
        }

        // Act - Try to deserialize
        var deserialized = XferConvert.Deserialize<SimpleTestClass>(xferContent);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.Name, deserialized.Name);
        Assert.AreEqual(original.Age, deserialized.Age);
        Assert.AreEqual(original.IsActive, deserialized.IsActive);
    }

    [TestMethod]
    public void Deserialize_CustomClass_WithTypeParameter_ShouldWork()
    {
        // Arrange
        var original = new SimpleTestClass
        {
            Name = "Type Test User",
            Age = 30,
            IsActive = false
        };

        var xferContent = XferConvert.Serialize(original, Formatting.Pretty);
        Console.WriteLine("XferLang content for Type parameter test:");
        Console.WriteLine(xferContent);

        // Act - Try to deserialize using Type parameter (like the web service does)
        var deserialized = XferConvert.Deserialize(xferContent, typeof(SimpleTestClass));

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.IsInstanceOfType(deserialized, typeof(SimpleTestClass));

        var typed = (SimpleTestClass)deserialized;
        Assert.AreEqual(original.Name, typed.Name);
        Assert.AreEqual(original.Age, typed.Age);
        Assert.AreEqual(original.IsActive, typed.IsActive);
    }
}
