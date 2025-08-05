using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Configuration;
using System.Text;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class XferConvertAsyncTests
{
    private TestData _testData = null!;
    private XferSerializerSettings _settings = null!;

    [TestInitialize]
    public void Setup()
    {
        _testData = new TestData
        {
            Name = "Alice Johnson",
            Age = 30,
            IsActive = true,
            CreatedAt = new DateTime(2023, 12, 25, 10, 30, 45, DateTimeKind.Utc),
            Tags = new List<string> { "developer", "async", "xferlang" },
            Metadata = new Dictionary<string, object>
            {
                ["version"] = "1.0",
                ["priority"] = 5
            }
        };

        _settings = new XferSerializerSettings();
    }

    [TestMethod]
    public async Task SerializeToFileAsync_ValidObject_CreatesFileSuccessfully()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            await XferConvert.SerializeToFileAsync(_testData, tempFile, _settings, Formatting.Indented);

            // Assert
            Assert.IsTrue(File.Exists(tempFile));
            var content = await File.ReadAllTextAsync(tempFile);
            Assert.IsTrue(content.Contains("Alice Johnson"));
            Assert.IsTrue(content.Contains("30"));
            Assert.IsTrue(content.Contains("~true"));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [TestMethod]
    public async Task DeserializeFromFileAsync_ValidFile_DeserializesCorrectly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();

        try
        {
            await XferConvert.SerializeToFileAsync(_testData, tempFile, _settings, Formatting.Indented);

            // Act
            var result = await XferConvert.DeserializeFromFileAsync<TestData>(tempFile, _settings);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_testData.Name, result.Name);
            Assert.AreEqual(_testData.Age, result.Age);
            Assert.AreEqual(_testData.IsActive, result.IsActive);
            CollectionAssert.AreEqual(_testData.Tags.ToList(), result.Tags.ToList());
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [TestMethod]
    public async Task SerializeAsync_WithTextWriter_WritesCorrectly()
    {
        // Arrange
        using var stringWriter = new StringWriter();

        // Act
        await XferConvert.SerializeAsync(_testData, stringWriter, _settings, Formatting.Indented);

        // Assert
        var result = stringWriter.ToString();
        Assert.IsTrue(result.Contains("Alice Johnson"));
        Assert.IsTrue(result.Contains("30"));
        Assert.IsTrue(result.Contains("~true"));
    }

    [TestMethod]
    public async Task DeserializeAsync_WithTextReader_DeserializesCorrectly()
    {
        // Arrange
        using var stringWriter = new StringWriter();
        await XferConvert.SerializeAsync(_testData, stringWriter, _settings, Formatting.Indented);
        var xferContent = stringWriter.ToString();

        using var stringReader = new StringReader(xferContent);

        // Act
        var result = await XferConvert.DeserializeAsync<TestData>(stringReader, _settings);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_testData.Name, result.Name);
        Assert.AreEqual(_testData.Age, result.Age);
        Assert.AreEqual(_testData.IsActive, result.IsActive);
    }

    [TestMethod]
    public async Task SerializeToStreamAsync_ValidObject_WritesToStream()
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act
        await XferConvert.SerializeToStreamAsync(_testData, stream, _settings, Formatting.Indented);

        // Assert
        Assert.IsTrue(stream.Length > 0);

        stream.Position = 0;
        var content = await new StreamReader(stream).ReadToEndAsync();
        Assert.IsTrue(content.Contains("Alice Johnson"));
    }

    [TestMethod]
    public async Task DeserializeFromStreamAsync_ValidStream_DeserializesCorrectly()
    {
        // Arrange
        using var stream = new MemoryStream();
        await XferConvert.SerializeToStreamAsync(_testData, stream, _settings, Formatting.Indented);
        stream.Position = 0;

        // Act
        var result = await XferConvert.DeserializeFromStreamAsync<TestData>(stream, _settings);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_testData.Name, result.Name);
        Assert.AreEqual(_testData.Age, result.Age);
        Assert.AreEqual(_testData.IsActive, result.IsActive);
    }

    [TestMethod]
    public async Task TrySerializeToFileAsync_ValidObject_ReturnsTrue()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            var result = await XferConvert.TrySerializeToFileAsync(_testData, tempFile);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [TestMethod]
    public async Task TryDeserializeFromFileAsync_ValidFile_ReturnsSuccessWithData()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();

        try
        {
            await XferConvert.SerializeToFileAsync(_testData, tempFile, _settings, Formatting.Indented);

            // Act
            var (success, result) = await XferConvert.TryDeserializeFromFileAsync<TestData>(tempFile);

            // Assert
            Assert.IsTrue(success);
            Assert.IsNotNull(result);
            Assert.AreEqual(_testData.Name, result!.Name);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [TestMethod]
    public async Task TryDeserializeFromFileAsync_NonExistentFile_ReturnsFalseWithDefault()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".xfer");

        // Act
        var (success, result) = await XferConvert.TryDeserializeFromFileAsync<TestData>(nonExistentFile);

        // Assert
        Assert.IsFalse(success);
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task SerializeToFileAsync_WithCancellation_CanBeCancelled()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        try
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                async () => await XferConvert.SerializeToFileAsync(_testData, tempFile, _settings, Formatting.Indented, cts.Token));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [TestMethod]
    public async Task DeserializeFromFileAsync_WithCancellation_CanBeCancelled()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        await XferConvert.SerializeToFileAsync(_testData, tempFile, _settings, Formatting.Indented);

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        try
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                async () => await XferConvert.DeserializeFromFileAsync<TestData>(tempFile, _settings, cts.Token));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [TestMethod]
    public async Task SerializeAsync_WithCancellation_CanBeCancelled()
    {
        // Arrange
        using var stringWriter = new StringWriter();
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(
            async () => await XferConvert.SerializeAsync(_testData, stringWriter, _settings, Formatting.Indented, cts.Token));
    }

    [TestMethod]
    public async Task DeserializeAsync_WithCancellation_CanBeCancelled()
    {
        // Arrange
        using var stringWriter = new StringWriter();
        await XferConvert.SerializeAsync(_testData, stringWriter, _settings, Formatting.Indented);
        var xferContent = stringWriter.ToString();

        using var stringReader = new StringReader(xferContent);
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(
            async () => await XferConvert.DeserializeAsync<TestData>(stringReader, _settings, cts.Token));
    }

    [TestMethod]
    public async Task ConcurrentOperations_MultipleAsyncCalls_ExecuteSuccessfully()
    {
        // Arrange
        var tasks = new List<Task>();
        var tempFiles = new List<string>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var tempFile = Path.GetTempFileName();
            tempFiles.Add(tempFile);

            var testData = new TestData
            {
                Name = $"User {i}",
                Age = 25 + i,
                IsActive = i % 2 == 0,
                CreatedAt = DateTime.UtcNow.AddHours(-i),
                Tags = new List<string> { $"tag{i}", "async" }
            };

            tasks.Add(Task.Run(async () =>
            {
                await XferConvert.SerializeToFileAsync(testData, tempFile, new XferSerializerSettings(), Formatting.Indented);
                var result = await XferConvert.DeserializeFromFileAsync<TestData>(tempFile, new XferSerializerSettings());
                Assert.AreEqual(testData.Name, result!.Name);
            }));
        }

        try
        {
            // Assert
            await Task.WhenAll(tasks);

            // Verify all files were created
            foreach (var file in tempFiles)
            {
                Assert.IsTrue(File.Exists(file));
            }
        }
        finally
        {
            // Cleanup
            foreach (var file in tempFiles)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }
    }

    [TestMethod]
    public async Task LargeObjectSerialization_BigData_HandlesEfficiently()
    {
        // Arrange
        var largeData = new TestData
        {
            Name = "Large Test",
            Age = 42,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Tags = Enumerable.Range(0, 1000).Select(i => $"tag_{i}").ToList(),
            Metadata = Enumerable.Range(0, 500)
                .ToDictionary(i => $"key_{i}", i => (object)$"value_{i}")
        };

        var tempFile = Path.GetTempFileName();

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            await XferConvert.SerializeToFileAsync(largeData, tempFile, _settings, Formatting.Indented);
            var result = await XferConvert.DeserializeFromFileAsync<TestData>(tempFile, _settings);

            stopwatch.Stop();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(largeData.Name, result!.Name);
            Assert.AreEqual(1000, result.Tags.Count);
            Assert.AreEqual(500, result.Metadata.Count);

            // Performance assertion - should complete within reasonable time
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 5000,
                $"Large object serialization took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [TestMethod]
    public async Task FileOperations_WithDirectoryCreation_CreatesDirectoriesAutomatically()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var nestedDir = Path.Combine(tempDir, "nested", "deeper");
        var filePath = Path.Combine(nestedDir, "test.xfer");

        try
        {
            // Act
            await XferConvert.SerializeToFileAsync(_testData, filePath, _settings, Formatting.Indented);

            // Assert
            Assert.IsTrue(Directory.Exists(nestedDir));
            Assert.IsTrue(File.Exists(filePath));

            var result = await XferConvert.DeserializeFromFileAsync<TestData>(filePath, _settings);
            Assert.AreEqual(_testData.Name, result!.Name);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [TestMethod]
    public async Task AsyncOperations_WithDifferentEncodings_HandleCorrectly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var unicodeData = new TestData
        {
            Name = "🌟 Unicode Test 中文 العربية 🚀",
            Age = 25,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Tags = new List<string> { "unicode", "测试", "🎯" }
        };

        try
        {
            // Act
            await XferConvert.SerializeToFileAsync(unicodeData, tempFile, _settings, Formatting.Indented);
            var result = await XferConvert.DeserializeFromFileAsync<TestData>(tempFile, _settings);

            // Assert
            Assert.AreEqual(unicodeData.Name, result!.Name);
            CollectionAssert.AreEqual(unicodeData.Tags.ToList(), result.Tags.ToList());
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

public class TestData
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}
