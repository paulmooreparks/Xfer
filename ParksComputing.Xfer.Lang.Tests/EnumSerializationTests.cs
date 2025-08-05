using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;

namespace ParksComputing.Xfer.Lang.Tests;

public enum TestEnumLocal
{
    None,
    Indented,
    Spaced,
    Pretty
}

public class TestClass
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public TestEnumLocal TestEnum { get; set; }
}

[TestClass]
public class EnumSerializationTests
{
    [TestMethod]
    public void SerializeEnum_TestEnum_ShowsCurrentBehavior()
    {
        // Test individual enum values
        var none = TestEnumLocal.None;
        var indented = TestEnumLocal.Indented;
        var spaced = TestEnumLocal.Spaced;
        var pretty = TestEnumLocal.Pretty;

        var noneXfer = XferConvert.Serialize(none);
        var indentedXfer = XferConvert.Serialize(indented);
        var spacedXfer = XferConvert.Serialize(spaced);
        var prettyXfer = XferConvert.Serialize(pretty);

        Console.WriteLine("Current enum serialization:");
        Console.WriteLine($"TestEnum.None: {noneXfer}");
        Console.WriteLine($"TestEnum.Indented: {indentedXfer}");
        Console.WriteLine($"TestEnum.Spaced: {spacedXfer}");
        Console.WriteLine($"TestEnum.Pretty: {prettyXfer}");

        // Verify they are IdentifierElements with colon delimiters
        Assert.AreEqual(":None:", noneXfer);
        Assert.AreEqual(":Indented:", indentedXfer);
        Assert.AreEqual(":Spaced:", spacedXfer);
        Assert.AreEqual(":Pretty:", prettyXfer);

        // Test in a class context
        var testData = new TestClass
        {
            Name = "Test User",
            Age = 25,
            TestEnum = TestEnumLocal.Spaced
        };

        var serialized = XferConvert.Serialize(testData, Formatting.Pretty);
        Console.WriteLine("\nTestClass with TestEnum.Spaced:");
        Console.WriteLine(serialized);

        // Test roundtrip through class
        var deserializedClass = XferConvert.Deserialize<TestClass>(serialized);
        Assert.IsNotNull(deserializedClass);
        Assert.AreEqual(testData.Name, deserializedClass.Name);
        Assert.AreEqual(testData.Age, deserializedClass.Age);
        Assert.AreEqual(testData.TestEnum, deserializedClass.TestEnum);
    }

    [TestMethod]
    public void RoundtripEnum_InClass_ShouldWork()
    {
        // Test enum roundtrip within a class/object context
        var testData = new TestClass
        {
            Name = "Test User",
            Age = 25,
            TestEnum = TestEnumLocal.Pretty
        };

        var serialized = XferConvert.Serialize(testData);
        Console.WriteLine($"Serialized: {serialized}");

        var deserialized = XferConvert.Deserialize<TestClass>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(testData.TestEnum, deserialized.TestEnum);
        Console.WriteLine($"Class roundtrip successful: {testData.TestEnum} -> {deserialized.TestEnum}");
    }

    [TestMethod]
    public void RoundtripEnum_InArray_ShouldWork()
    {
        // Test enum roundtrip within an array context
        var enumArray = new[] { TestEnumLocal.None, TestEnumLocal.Indented, TestEnumLocal.Spaced, TestEnumLocal.Pretty };

        Console.WriteLine($"Array type: {enumArray.GetType()}");
        Console.WriteLine($"Element type: {enumArray.GetType().GetElementType()}");
        Console.WriteLine($"Is Array: {enumArray is Array}");
        Console.WriteLine($"Element type is enum: {enumArray.GetType().GetElementType()?.IsEnum}");

        var serialized = XferConvert.Serialize(enumArray);
        Console.WriteLine($"Serialized enum array: {serialized}");

        var deserialized = XferConvert.Deserialize<TestEnumLocal[]>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(enumArray.Length, deserialized.Length);
        for (int i = 0; i < enumArray.Length; i++)
        {
            Assert.AreEqual(enumArray[i], deserialized[i]);
        }

        Console.WriteLine("Array roundtrip successful");
    }
}
