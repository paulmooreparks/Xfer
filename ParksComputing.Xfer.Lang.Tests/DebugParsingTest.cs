using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;

namespace ParksComputing.Xfer.Lang.Tests;

public enum TestEnum
{
    None,
    Indented,
    Spaced,
    Pretty
}

public class TestObjectWithEnum
{
    public TestEnum TestEnum { get; set; }
    public string Message { get; set; } = string.Empty;
}

[TestClass]
public class DebugParsingTest
{
    [TestMethod]
    public void TestSimpleEnumInObject()
    {
        // Test a minimal case to debug the parsing issue
        var xferText = "{ TestEnum :Spaced: }";

        Console.WriteLine($"Parsing: {xferText}");

        try
        {
            var document = XferParser.Parse(xferText);
            var serialized = document.Root.ToXfer(Formatting.Pretty);
            Console.WriteLine($"Parsed successfully: {serialized}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Parse failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [TestMethod]
    public void TestJustIdentifierElement()
    {
        // Test parsing just an identifier element to see if that works
        var xferText = ":Spaced:";

        Console.WriteLine($"Parsing standalone identifier: {xferText}");

        try
        {
            var document = XferParser.Parse(xferText);
            var serialized = document.Root.ToXfer(Formatting.Pretty);
            Console.WriteLine($"Parsed successfully: {serialized}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Parse failed: {ex.Message}");
        }
    }

    [TestMethod]
    public void TestCompleteEnumRoundTrip()
    {
        // Test complete round-trip: object with enum -> serialize -> parse -> deserialize
        var original = new TestObjectWithEnum { TestEnum = TestEnum.Spaced, Message = "Hello" };

        Console.WriteLine($"Original object: TestEnum={original.TestEnum}, Message={original.Message}");

        // Serialize to XferLang
        var xferText = XferConvert.Serialize(original);
        Console.WriteLine($"Serialized XferLang: {xferText}");

        // Parse the XferLang back to document
        var document = XferParser.Parse(xferText);
        var reparsed = document.Root.ToXfer(Formatting.Pretty);
        Console.WriteLine($"Reparsed XferLang: {reparsed}");

        // Deserialize back to object
        var result = XferConvert.Deserialize<TestObjectWithEnum>(xferText);
        Assert.IsNotNull(result);
        Console.WriteLine($"Deserialized: TestEnum={result.TestEnum}, Message={result.Message}");

        // Verify the enum value matches
        Assert.AreEqual(TestEnum.Spaced, result.TestEnum);
        Assert.AreEqual("Hello", result.Message);
    }
}
