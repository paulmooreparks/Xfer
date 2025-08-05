using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class NullableTypeTests
{
    public class TestClassWithNullables
    {
        public int? NullableInt { get; set; }
        public decimal? NullableDecimal { get; set; }
        public bool? NullableBool { get; set; }
        public DateTime? NullableDateTime { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [TestMethod]
    public void SerializeDeserialize_NullableProperties_HandlesCorrectly()
    {
        // Arrange
        var original = new TestClassWithNullables
        {
            Name = "Test",
            NullableInt = 42,
            NullableDecimal = 123.45m,
            NullableBool = true,
            NullableDateTime = new DateTime(2023, 12, 25)
        };

        // Act - Serialize
        var xferContent = XferConvert.Serialize(original, Formatting.Pretty);
        Console.WriteLine("Serialized:");
        Console.WriteLine(xferContent);

        // Act - Deserialize
        var deserialized = XferConvert.Deserialize<TestClassWithNullables>(xferContent);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.Name, deserialized.Name);
        Assert.AreEqual(original.NullableInt, deserialized.NullableInt);
        Assert.AreEqual(original.NullableDecimal, deserialized.NullableDecimal);
        Assert.AreEqual(original.NullableBool, deserialized.NullableBool);
        Assert.AreEqual(original.NullableDateTime, deserialized.NullableDateTime);
    }

    [TestMethod]
    public void SerializeDeserialize_NullValues_HandlesCorrectly()
    {
        // Arrange
        var original = new TestClassWithNullables
        {
            Name = "Test with nulls",
            NullableInt = null,
            NullableDecimal = null,
            NullableBool = null,
            NullableDateTime = null
        };

        // Act - Serialize
        var xferContent = XferConvert.Serialize(original, Formatting.Pretty);
        Console.WriteLine("Serialized:");
        Console.WriteLine(xferContent);

        // Act - Deserialize
        var deserialized = XferConvert.Deserialize<TestClassWithNullables>(xferContent);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.Name, deserialized.Name);
        Assert.AreEqual(original.NullableInt, deserialized.NullableInt);
        Assert.AreEqual(original.NullableDecimal, deserialized.NullableDecimal);
        Assert.AreEqual(original.NullableBool, deserialized.NullableBool);
        Assert.AreEqual(original.NullableDateTime, deserialized.NullableDateTime);
    }

    [TestMethod]
    public async Task SerializeDeserializeAsync_NullableProperties_HandlesCorrectly()
    {
        // Arrange
        var original = new TestClassWithNullables
        {
            Name = "Async Test",
            NullableInt = 100,
            NullableDecimal = null,
            NullableBool = false,
            NullableDateTime = null
        };

        var tempFile = Path.GetTempFileName();
        try
        {
            // Act - Async Serialize
            await XferConvert.SerializeToFileAsync(original, tempFile, Formatting.Pretty);

            // Act - Async Deserialize
            var deserialized = await XferConvert.DeserializeFromFileAsync<TestClassWithNullables>(tempFile);

            // Assert
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(original.Name, deserialized.Name);
            Assert.AreEqual(original.NullableInt, deserialized.NullableInt);
            Assert.AreEqual(original.NullableDecimal, deserialized.NullableDecimal);
            Assert.AreEqual(original.NullableBool, deserialized.NullableBool);
            Assert.AreEqual(original.NullableDateTime, deserialized.NullableDateTime);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
