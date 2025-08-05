using ParksComputing.Xfer.Lang;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Xfer.AsyncDemo
{
    public class SimpleData
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    public class AsyncUpgradeDemo
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== XferLang Async Upgrade Demo ===\n");

            var testData = new SimpleData
            {
                Name = "Async Demo User",
                Age = 30,
                IsActive = true,
                Created = DateTime.Now,
                Tags = new List<string> { "async", "upgrade", "demo" }
            };

            await DemonstrateAsyncFileOperations(testData);
            await DemonstrateAsyncStreamOperations(testData);
            await DemonstrateTryPatternAsync(testData);

            Console.WriteLine("\n✅ All async upgrade demonstrations completed successfully!");
        }

        private static async Task DemonstrateAsyncFileOperations(SimpleData data)
        {
            Console.WriteLine("1. File Operations Async Demo");
            Console.WriteLine("-----------------------------");

            var tempFile = Path.GetTempFileName();
            try
            {
                // Async file serialization
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                await XferConvert.SerializeToFileAsync(data, tempFile, Formatting.Pretty);
                stopwatch.Stop();

                Console.WriteLine($"✓ Async file serialization: {stopwatch.ElapsedMilliseconds}ms");

                // Async file deserialization
                stopwatch.Restart();
                var deserializedData = await XferConvert.DeserializeFromFileAsync<SimpleData>(tempFile);
                stopwatch.Stop();

                Console.WriteLine($"✓ Async file deserialization: {stopwatch.ElapsedMilliseconds}ms");

                if (deserializedData != null)
                {
                    var isEqual = deserializedData.Name == data.Name &&
                                 deserializedData.Age == data.Age &&
                                 deserializedData.IsActive == data.IsActive;

                    Console.WriteLine($"✓ Data integrity: {(isEqual ? "VERIFIED" : "FAILED")}");
                    Console.WriteLine($"  Name: {deserializedData.Name}");
                    Console.WriteLine($"  Age: {deserializedData.Age}");
                    Console.WriteLine($"  IsActive: {deserializedData.IsActive}");
                    Console.WriteLine($"  Tags: [{string.Join(", ", deserializedData.Tags)}]");
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }

            Console.WriteLine();
        }

        private static async Task DemonstrateAsyncStreamOperations(SimpleData data)
        {
            Console.WriteLine("2. Stream Operations Async Demo");
            Console.WriteLine("-------------------------------");

            // Async stream serialization
            using var memoryStream = new MemoryStream();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await XferConvert.SerializeToStreamAsync(data, memoryStream, Formatting.Pretty);
            stopwatch.Stop();

            Console.WriteLine($"✓ Async stream serialization: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"✓ Stream size: {memoryStream.Length} bytes");

            // Reset stream position for reading
            memoryStream.Position = 0;

            // Async stream deserialization
            stopwatch.Restart();
            var deserializedData = await XferConvert.DeserializeFromStreamAsync<SimpleData>(memoryStream);
            stopwatch.Stop();

            Console.WriteLine($"✓ Async stream deserialization: {stopwatch.ElapsedMilliseconds}ms");

            if (deserializedData != null)
            {
                var isEqual = deserializedData.Name == data.Name &&
                             deserializedData.Age == data.Age &&
                             deserializedData.IsActive == data.IsActive;

                Console.WriteLine($"✓ Stream roundtrip: {(isEqual ? "SUCCESS" : "FAILED")}");
            }

            Console.WriteLine();
        }

        private static async Task DemonstrateTryPatternAsync(SimpleData data)
        {
            Console.WriteLine("3. Try-Pattern Async Demo");
            Console.WriteLine("-------------------------");

            var tempFile = Path.GetTempFileName();
            try
            {
                // Try pattern file operations
                var serializeSuccess = await XferConvert.TrySerializeToFileAsync(data, tempFile);
                Console.WriteLine($"✓ TrySerializeToFileAsync: {serializeSuccess}");

                var (deserializeSuccess, result) = await XferConvert.TryDeserializeFromFileAsync<SimpleData>(tempFile);
                Console.WriteLine($"✓ TryDeserializeFromFileAsync: Success={deserializeSuccess}");

                if (deserializeSuccess && result != null)
                {
                    Console.WriteLine($"  Deserialized name: {result.Name}");
                }

                // Test with non-existent file
                var (failureSuccess, _) = await XferConvert.TryDeserializeFromFileAsync<SimpleData>("non-existent.xfer");
                Console.WriteLine($"✓ TryDeserializeFromFileAsync (non-existent): Success={failureSuccess}");

                // Test try pattern with TextWriter/TextReader
                using var writer = new StringWriter();
                var trySerializeSuccess = await XferConvert.TrySerializeAsync(data, writer);
                Console.WriteLine($"✓ TrySerializeAsync (TextWriter): {trySerializeSuccess}");

                if (trySerializeSuccess)
                {
                    var serializedText = writer.ToString();
                    using var reader = new StringReader(serializedText);
                    var (tryDeserializeSuccess, tryResult) = await XferConvert.TryDeserializeAsync<SimpleData>(reader);
                    Console.WriteLine($"✓ TryDeserializeAsync (TextReader): Success={tryDeserializeSuccess}");
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }

            Console.WriteLine();
        }
    }
}
