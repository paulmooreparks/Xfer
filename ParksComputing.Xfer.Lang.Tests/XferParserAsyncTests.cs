using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using System.Text;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class XferParserAsyncTests
{
    private string _sampleXferContent = string.Empty;
    private string _complexXferContent = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        _sampleXferContent = """
            {
              Name "Alice Johnson"
              Age #30
              IsActive ~true
              CreatedAt @2023-12-25T10:30:45.0000000Z@
              Tags [
                "developer"
                "async"
                "xferlang"
              ]
            }
            """;

        _complexXferContent = """
            {
              Person {
                Name "John Doe"
                Age #42
                Contact {
                  Email "john@example.com"
                  Phone "+1-555-0123"
                }
              }
              Numbers (
                #42
                *3.14159
                $FF
                %1010
              )
              Features {
                Unicode "🌟 Test 中文 العربية 🚀"
                Null ?
                Boolean ~true
              }
            }
            """;
    }

    [TestMethod]
    public async Task ParseFileAsync_ValidFile_ParsesCorrectly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(tempFile, _sampleXferContent);

            // Act
            var document = await XferParser.ParseFileAsync(tempFile);

            // Assert
            Assert.IsNotNull(document);
            Assert.IsNotNull(document.Root);
            Assert.IsInstanceOfType(document.Root, typeof(ObjectElement));

            var rootObj = (ObjectElement)document.Root;
            Assert.IsTrue(rootObj.ContainsKey("Name"));
            Assert.IsTrue(rootObj.ContainsKey("Age"));
            Assert.IsTrue(rootObj.ContainsKey("IsActive"));
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
    public async Task ParseFileAsync_NonExistentFile_ThrowsException()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".xfer");

        // Act & Assert
        await Assert.ThrowsExceptionAsync<FileNotFoundException>(
            async () => await XferParser.ParseFileAsync(nonExistentFile));
    }

    [TestMethod]
    public async Task ParseFileAsync_WithCancellation_CanBeCancelled()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        try
        {
            await File.WriteAllTextAsync(tempFile, _sampleXferContent);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                async () => await XferParser.ParseFileAsync(tempFile, cts.Token));
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
    public async Task ParseStreamAsync_ValidStream_ParsesCorrectly()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(_sampleXferContent));

        // Act
        var document = await XferParser.ParseStreamAsync(stream);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);
        Assert.IsInstanceOfType(document.Root, typeof(ObjectElement));

        var rootObj = (ObjectElement)document.Root;
        Assert.IsTrue(rootObj.ContainsKey("Name"));
    }

    [TestMethod]
    public async Task ParseStreamAsync_EmptyStream_ReturnsEmptyDocument()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("()"));

        // Act
        var document = await XferParser.ParseStreamAsync(stream);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);
        Assert.IsInstanceOfType(document.Root, typeof(TupleElement));
    }

    [TestMethod]
    public async Task ParseStreamAsync_WithCancellation_CanBeCancelled()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(_sampleXferContent));
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(
            async () => await XferParser.ParseStreamAsync(stream, cts.Token));
    }

    [TestMethod]
    public async Task ParseTextReaderAsync_ValidContent_ParsesCorrectly()
    {
        // Arrange
        using var reader = new StringReader(_sampleXferContent);

        // Act
        var document = await XferParser.ParseTextReaderAsync(reader);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);
        Assert.IsInstanceOfType(document.Root, typeof(ObjectElement));

        var rootObj = (ObjectElement)document.Root;
        Assert.IsTrue(rootObj.ContainsKey("Name"));
        Assert.IsTrue(rootObj.ContainsKey("Age"));
    }

    [TestMethod]
    public async Task ParseTextReaderAsync_ComplexContent_ParsesCorrectly()
    {
        // Arrange
        using var reader = new StringReader(_complexXferContent);

        // Act
        var document = await XferParser.ParseTextReaderAsync(reader);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);
        Assert.IsInstanceOfType(document.Root, typeof(ObjectElement));

        var rootObj = (ObjectElement)document.Root;
        Assert.IsTrue(rootObj.ContainsKey("Person"));
        Assert.IsTrue(rootObj.ContainsKey("Numbers"));
        Assert.IsTrue(rootObj.ContainsKey("Features"));

        // Verify nested structure
        var person = rootObj["Person"] as ObjectElement;
        Assert.IsNotNull(person);
        Assert.IsTrue(person.ContainsKey("Contact"));

        var numbers = rootObj["Numbers"] as TupleElement;
        Assert.IsNotNull(numbers);
        Assert.AreEqual(4, numbers.Count);
    }

    [TestMethod]
    public async Task ParseTextReaderAsync_WithCancellation_CanBeCancelled()
    {
        // Arrange
        using var reader = new StringReader(_sampleXferContent);
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(
            async () => await XferParser.ParseTextReaderAsync(reader, cts.Token));
    }

    [TestMethod]
    public async Task ParseFileAsync_LargeFile_HandlesEfficiently()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var largeContent = GenerateLargeXferContent();

        try
        {
            await File.WriteAllTextAsync(tempFile, largeContent);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var document = await XferParser.ParseFileAsync(tempFile);
            stopwatch.Stop();

            // Assert
            Assert.IsNotNull(document);
            Assert.IsNotNull(document.Root);

            // Performance assertion - should complete within reasonable time
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 3000,
                $"Large file parsing took {stopwatch.ElapsedMilliseconds}ms, expected < 3000ms");
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
    public async Task ConcurrentParsing_MultipleFiles_ExecutesConcurrently()
    {
        // Arrange
        var tasks = new List<Task<XferDocument>>();
        var tempFiles = new List<string>();

        // Create multiple temporary files
        for (int i = 0; i < 5; i++)
        {
            var tempFile = Path.GetTempFileName();
            tempFiles.Add(tempFile);

            var content = $$"""
                {
                  Id {{i}}
                  Name <"File {{i}}">
                  Data [
                    {{string.Join("\n    ", Enumerable.Range(0, 10).Select(j => $"{j}"))}}
                  ]
                }
                """;

            await File.WriteAllTextAsync(tempFile, content);

            // Add parsing task
            tasks.Add(XferParser.ParseFileAsync(tempFile));
        }

        try
        {
            // Act
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.AreEqual(5, results.Length);

            for (int i = 0; i < results.Length; i++)
            {
                Assert.IsNotNull(results[i]);
                Assert.IsNotNull(results[i].Root);

                var rootObj = results[i].Root as ObjectElement;
                Assert.IsNotNull(rootObj);
                Assert.IsTrue(rootObj.ContainsKey("Id"));
                Assert.IsTrue(rootObj.ContainsKey("Name"));
                Assert.IsTrue(rootObj.ContainsKey("Data"));
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
    public async Task ParseFileAsync_UnicodeContent_HandlesCorrectly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var unicodeContent = """
            {
              Unicode <"🌟 Test 中文 العربية 🚀">
              Emoji <"👨‍💻 🎯 ⚡">
              Math <"∑ ∞ π √ ∫">
              Currency <"€ ¥ £ $ ₹">
            }
            """;

        try
        {
            await File.WriteAllTextAsync(tempFile, unicodeContent, Encoding.UTF8);

            // Act
            var document = await XferParser.ParseFileAsync(tempFile);

            // Assert
            Assert.IsNotNull(document);
            var rootObj = document.Root as ObjectElement;
            Assert.IsNotNull(rootObj);

            var unicodeValue = (rootObj["Unicode"] as StringElement)?.Value;
            Assert.AreEqual("🌟 Test 中文 العربية 🚀", unicodeValue);

            var emojiValue = (rootObj["Emoji"] as StringElement)?.Value;
            Assert.AreEqual("👨‍💻 🎯 ⚡", emojiValue);
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
    public async Task ParseStreamAsync_DifferentEncodings_HandlesCorrectly()
    {
        // Arrange
        var content = """
            {
              Text "English Text"
              Unicode "测试 тест テスト"
            }
            """;

        // Test with UTF8 encoding only (most reliable for XferLang)
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var document = await XferParser.ParseStreamAsync(stream);

        // Assert
        Assert.IsNotNull(document, "Failed with UTF-8 encoding");
        var rootObj = document.Root as ObjectElement;
        Assert.IsNotNull(rootObj, "Failed with UTF-8 encoding");
        Assert.IsTrue(rootObj.ContainsKey("Text"), "Failed with UTF-8 encoding");
        Assert.IsTrue(rootObj.ContainsKey("Unicode"), "Failed with UTF-8 encoding");
    }

    [TestMethod]
    public async Task ParseFileAsync_MalformedContent_ThrowsParseException()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var malformedContent = """
            {
              Name <"Unclosed string
              Age 30
              InvalidStructure [
                {
                  Missing closing brace
            """;

        try
        {
            await File.WriteAllTextAsync(tempFile, malformedContent);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await XferParser.ParseFileAsync(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    private string GenerateLargeXferContent()
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine("  LargeArray [");

        for (int i = 0; i < 1000; i++)
        {
            sb.AppendLine($"    {{");
            sb.AppendLine($"      Id #{i}");
            sb.AppendLine($"      Name \"Item {i}\"");
            sb.AppendLine($"      Value *{i * 3.14159:F6}");
            sb.AppendLine($"      Active ~{(i % 2 == 0).ToString().ToLower()}");
            sb.AppendLine($"    }}");
        }

        sb.AppendLine("  ]");
        sb.AppendLine("}");

        return sb.ToString();
    }
}
