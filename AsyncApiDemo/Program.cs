using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Configuration;

namespace AsyncApiDemo
{
    public class SampleData
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string[]? Tags { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== XferLang Async API Demo ===\n");

            var sampleData = new SampleData
            {
                Name = "Alice Johnson",
                Age = 30,
                IsActive = true,
                CreatedAt = DateTime.Now,
                Tags = new[] { "developer", "async", "xferlang" }
            };

            // Demo 1: File Operations
            await FileOperationsDemo(sampleData);

            // Demo 2: Stream Operations
            await StreamOperationsDemo(sampleData);

            // Demo 3: Error Handling with Try-Pattern
            await ErrorHandlingDemo(sampleData);

            // Demo 4: Cancellation Support
            await CancellationDemo(sampleData);

            // Demo 5: Performance Comparison
            await PerformanceDemo(sampleData);

            Console.WriteLine("\n=== All demos completed successfully! ===");
        }

        static async Task FileOperationsDemo(SampleData data)
        {
            Console.WriteLine("1. File Operations Demo");
            var tempFile = Path.GetTempFileName();

            try
            {
                // Serialize to file with pretty formatting
                await XferConvert.SerializeToFileAsync(data, tempFile, Formatting.Pretty);
                Console.WriteLine($"✓ Serialized to file: {tempFile}");

                // Read and display the content
                var content = await File.ReadAllTextAsync(tempFile);
                Console.WriteLine($"File content:\n{content}");

                // Deserialize from file
                var deserializedData = await XferConvert.DeserializeFromFileAsync<SampleData>(tempFile);
                Console.WriteLine($"✓ Deserialized: {deserializedData?.Name}, Age: {deserializedData?.Age}");
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }

            Console.WriteLine();
        }

        static async Task StreamOperationsDemo(SampleData data)
        {
            Console.WriteLine("2. Stream Operations Demo");

            // Memory stream demo
            using var memoryStream = new MemoryStream();

            // Serialize to stream
            await XferConvert.SerializeToStreamAsync(data, memoryStream, Formatting.Pretty);
            Console.WriteLine($"✓ Serialized to stream ({memoryStream.Length} bytes)");

            // Reset stream position for reading
            memoryStream.Position = 0;

            // Deserialize from stream
            var deserializedData = await XferConvert.DeserializeFromStreamAsync<SampleData>(memoryStream);
            Console.WriteLine($"✓ Deserialized from stream: {deserializedData?.Name}");

            // TextWriter/TextReader demo
            using var stringWriter = new StringWriter();
            await XferConvert.SerializeAsync(data, stringWriter, Formatting.Indented);

            var xferString = stringWriter.ToString();
            Console.WriteLine($"TextWriter result:\n{xferString}");

            using var stringReader = new StringReader(xferString);
            var readerResult = await XferConvert.DeserializeAsync<SampleData>(stringReader);
            Console.WriteLine($"✓ TextReader deserialized: {readerResult?.Name}");

            Console.WriteLine();
        }

        static async Task ErrorHandlingDemo(SampleData data)
        {
            Console.WriteLine("3. Error Handling Demo (Try-Pattern)");

            var tempFile = Path.GetTempFileName();

            try
            {
                // Test successful serialization
                var success = await XferConvert.TrySerializeToFileAsync(data, tempFile);
                Console.WriteLine($"✓ TrySerializeToFileAsync: {success}");

                // Test successful deserialization
                var (deserializeSuccess, result) = await XferConvert.TryDeserializeFromFileAsync<SampleData>(tempFile);
                Console.WriteLine($"✓ TryDeserializeFromFileAsync: {deserializeSuccess}, Name: {result?.Name}");

                // Test failure case - non-existent file
                var (failureSuccess, failureResult) = await XferConvert.TryDeserializeFromFileAsync<SampleData>("nonexistent.xfer");
                Console.WriteLine($"✓ Non-existent file handled gracefully: {failureSuccess}");
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }

            Console.WriteLine();
        }

        static async Task CancellationDemo(SampleData data)
        {
            Console.WriteLine("4. Cancellation Support Demo");

            using var cts = new CancellationTokenSource();
            var tempFile = Path.GetTempFileName();

            try
            {
                // Normal operation with cancellation token
                await XferConvert.SerializeToFileAsync(data, tempFile, cts.Token);
                Console.WriteLine("✓ Serialization completed (cancellation token provided)");

                var result = await XferConvert.DeserializeFromFileAsync<SampleData>(tempFile, cts.Token);
                Console.WriteLine($"✓ Deserialization completed: {result?.Name}");

                // Demonstrate cancellation (quick cancel for demo purposes)
                cts.CancelAfter(1); // Cancel almost immediately

                try
                {
                    await Task.Delay(10); // Let cancellation take effect
                    await XferConvert.SerializeToFileAsync(data, tempFile + ".cancelled", cts.Token);
                    Console.WriteLine("Operation should have been cancelled");
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("✓ Cancellation properly handled");
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
                if (File.Exists(tempFile + ".cancelled"))
                    File.Delete(tempFile + ".cancelled");
            }

            Console.WriteLine();
        }

        static async Task PerformanceDemo(SampleData data)
        {
            Console.WriteLine("5. Performance Demo");

            var iterations = 1000;
            var tasks = new Task[iterations];

            // Create multiple concurrent operations
            var startTime = DateTime.UtcNow;

            for (int i = 0; i < iterations; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    using var stream = new MemoryStream();
                    await XferConvert.SerializeToStreamAsync(data, stream);
                    stream.Position = 0;
                    await XferConvert.DeserializeFromStreamAsync<SampleData>(stream);
                });
            }

            await Task.WhenAll(tasks);
            var elapsed = DateTime.UtcNow - startTime;

            Console.WriteLine($"✓ Completed {iterations} concurrent serialize/deserialize operations in {elapsed.TotalMilliseconds:F1}ms");
            Console.WriteLine($"  Average: {elapsed.TotalMilliseconds / iterations:F3}ms per operation");

            Console.WriteLine();
        }
    }
}
