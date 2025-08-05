using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;
using Xfer.Data;
using System.Net.Http.Headers;
using System.Text;

namespace Xfer.Client;

internal class Program {
    static async Task Main(string[] args) {
        try {
            Console.WriteLine("=== XferLang Async Client Demo ===\n");

            // First, demonstrate async file operations (works offline)
            Console.WriteLine("1. Testing async file operations (offline demo)...");
            await TestAsyncFileOperationsOffline();

            // Then try web service demo (will show connection error if service is not running)
            Console.WriteLine("\n2. Attempting web service demo...");
            var client = new XferClient("https://localhost:7021");
            await RunDemoAsync(client);
        }
        catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static async Task RunDemoAsync(XferClient client) {
        Console.WriteLine("=== XferLang Web Service Demo ===\n");

        // GET request
        Console.WriteLine("1. Fetching sample data from server...");
        var sampleData = await client.GetAsync<SampleData>("api/sampledata");
        if (sampleData != null) {
            Console.WriteLine($"Received data:");
            Console.WriteLine($"  Name: {sampleData.Name}");
            Console.WriteLine($"  Age: {sampleData.Age}");
            Console.WriteLine($"  TestEnum: {sampleData.TestEnum}");
            Console.WriteLine($"  TimeOnly: {sampleData.TimeOnly}");
            Console.WriteLine($"  TimeSpan: {sampleData.TimeSpan}");
            Console.WriteLine($"  DateTime: {sampleData.DateTime}");
        } else {
            Console.WriteLine("Failed to retrieve data");
        }

        Console.WriteLine("\n2. Sending new data to server...");

        // POST request
        var postData = new SampleData {
            Name = "Bob Johnson",
            Age = 42,
            TimeSpan = new TimeSpan(90, 11, 43, 56),
            TimeOnly = new TimeOnly(23, 45, 56),
            TestEnum = TestEnum.Spaced,
            DateTime = DateTime.Now
        };

        var responseData = await client.PostAsync("api/sampledata", postData);
        if (responseData != null) {
            Console.WriteLine($"Server echoed back:");
            Console.WriteLine($"  Name: {responseData.Name}");
            Console.WriteLine($"  Age: {responseData.Age}");
            Console.WriteLine($"  TestEnum: {responseData.TestEnum}");
            Console.WriteLine($"  TimeOnly: {responseData.TimeOnly}");
            Console.WriteLine($"  TimeSpan: {responseData.TimeSpan}");
            Console.WriteLine($"  DateTime: {responseData.DateTime}");
        } else {
            Console.WriteLine("Failed to post data");
        }

        Console.WriteLine("\n3. Testing serialization roundtrip...");
        await TestSerializationRoundtrip(client);

        Console.WriteLine("\n4. Testing async file operations...");
        await TestAsyncFileOperations(client);
    }

    static async Task TestSerializationRoundtrip(XferClient client) {
        var testData = new SampleData {
            Name = "Test User",
            Age = 25,
            TimeSpan = TimeSpan.FromHours(8.5),
            TimeOnly = TimeOnly.FromTimeSpan(TimeSpan.FromHours(14.5)), // 2:30 PM
            TestEnum = TestEnum.Pretty,
            DateTime = new DateTime(2024, 1, 15, 10, 30, 0)
        };

        // Serialize locally using async method
        using var writer = new StringWriter();
        await XferConvert.SerializeAsync(testData, writer, Formatting.Pretty);
        var serialized = writer.ToString();
        Console.WriteLine($"Local async serialization:\n{serialized}");

        // Send to server and get back
        var result = await client.PostAsync("api/sampledata", testData);

        // Verify roundtrip
        var isEqual = result != null &&
                     result.Name == testData.Name &&
                     result.Age == testData.Age &&
                     result.TimeSpan == testData.TimeSpan &&
                     result.TimeOnly == testData.TimeOnly &&
                     result.TestEnum == testData.TestEnum &&
                     Math.Abs((result.DateTime - testData.DateTime).TotalSeconds) < 1; // Allow for small time differences

        Console.WriteLine($"Roundtrip test: {(isEqual ? "PASSED" : "FAILED")}");
    }

    static async Task TestAsyncFileOperations(XferClient client) {
        var testData = new SampleData {
            Name = "File Test User",
            Age = 35,
            TimeSpan = TimeSpan.FromMinutes(123),
            TimeOnly = TimeOnly.FromTimeSpan(TimeSpan.FromHours(9.75)), // 9:45 AM
            TestEnum = TestEnum.Spaced,
            DateTime = DateTime.Now,
            Salary = 95000.75m,
            IsActive = true,
            Tags = new List<string> { "async", "file", "demo" },
            Metadata = new Dictionary<string, object> {
                { "fileTest", true },
                { "asyncDemo", 1.0 } // Changed to double
            }
        };

        var tempFile = Path.GetTempFileName();
        try {
            Console.WriteLine("Testing async file serialization...");

            // Serialize to file using async method
            await XferConvert.SerializeToFileAsync(testData, tempFile, Formatting.Pretty);
            Console.WriteLine($"✓ Data serialized to: {tempFile}");

            // Read file content to show what was written
            var fileContent = await File.ReadAllTextAsync(tempFile);
            Console.WriteLine($"File content preview (first 200 chars):\n{fileContent.Substring(0, Math.Min(200, fileContent.Length))}...");

            // Deserialize from file using async method
            var deserializedData = await XferConvert.DeserializeFromFileAsync<SampleData>(tempFile);

            if (deserializedData != null) {
                Console.WriteLine($"✓ Data deserialized from file:");
                Console.WriteLine($"  Name: {deserializedData.Name}");
                Console.WriteLine($"  Age: {deserializedData.Age}");
                Console.WriteLine($"  TestEnum: {deserializedData.TestEnum}");
                Console.WriteLine($"  Tags: [{string.Join(", ", deserializedData.Tags)}]");

                // Verify data integrity
                var isIntact = deserializedData.Name == testData.Name &&
                              deserializedData.Age == testData.Age &&
                              deserializedData.TestEnum == testData.TestEnum &&
                              deserializedData.Tags.SequenceEqual(testData.Tags);

                Console.WriteLine($"✓ File operation roundtrip: {(isIntact ? "PASSED" : "FAILED")}");
            } else {
                Console.WriteLine("✗ Failed to deserialize data from file");
            }

            // Test try-pattern async methods
            Console.WriteLine("\nTesting try-pattern async methods...");
            var tryResult = await XferConvert.TryDeserializeFromFileAsync<SampleData>(tempFile);
            Console.WriteLine($"✓ TryDeserializeFromFileAsync: Success={tryResult.Success}");

        } finally {
            // Cleanup
            if (File.Exists(tempFile)) {
                File.Delete(tempFile);
                Console.WriteLine("✓ Temporary file cleaned up");
            }
        }
    }

    static async Task TestAsyncFileOperationsOffline() {
        var testData = new SampleData {
            Name = "Async Demo User",
            Age = 28,
            TimeSpan = TimeSpan.FromHours(7.5),
            TimeOnly = TimeOnly.FromTimeSpan(TimeSpan.FromHours(15.25)), // 3:15 PM
            TestEnum = TestEnum.Pretty,
            DateTime = DateTime.Now,
            Salary = 87500.50m, // This should work as nullable decimal
            IsActive = true,
            Tags = new List<string> { "async", "demo", "offline" },
            Metadata = new Dictionary<string, object> {
                { "demoType", "async-file-operations" },
                { "version", 1.0 }, // Changed to double instead of string
                { "performance", true } // Changed to bool instead of string
            }
        };

        var tempFile = Path.GetTempFileName();
        try {
            Console.WriteLine("✓ Starting async file operations demo...");

            // Measure performance of async serialization
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await XferConvert.SerializeToFileAsync(testData, tempFile, Formatting.Pretty);
            stopwatch.Stop();

            Console.WriteLine($"✓ Async serialization completed in {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"  File saved to: {tempFile}");

            // Show file content
            var fileContent = await File.ReadAllTextAsync(tempFile);
            Console.WriteLine($"  File size: {fileContent.Length} characters");
            Console.WriteLine("  Content preview:");
            Console.WriteLine(fileContent.Substring(0, Math.Min(300, fileContent.Length)) + "...");

            // Test async deserialization
            stopwatch.Restart();
            var deserializedData = await XferConvert.DeserializeFromFileAsync<SampleData>(tempFile);
            stopwatch.Stop();

            Console.WriteLine($"\n✓ Async deserialization completed in {stopwatch.ElapsedMilliseconds}ms");

            if (deserializedData != null) {
                Console.WriteLine("✓ Data integrity verified:");
                Console.WriteLine($"  Name: {deserializedData.Name}");
                Console.WriteLine($"  Age: {deserializedData.Age}");
                Console.WriteLine($"  TestEnum: {deserializedData.TestEnum}");
                Console.WriteLine($"  Salary: {deserializedData.Salary:C}");
                Console.WriteLine($"  Tags: [{string.Join(", ", deserializedData.Tags)}]");
                Console.WriteLine($"  Metadata entries: {deserializedData.Metadata.Count}");

                // Verify data match
                var isMatch = deserializedData.Name == testData.Name &&
                             deserializedData.Age == testData.Age &&
                             deserializedData.TestEnum == testData.TestEnum &&
                             deserializedData.Salary == testData.Salary;

                Console.WriteLine($"  Data integrity check: {(isMatch ? "PASSED" : "FAILED")}");
            }

            // Test try-pattern methods
            Console.WriteLine("\n✓ Testing try-pattern async methods...");
            var (success, result) = await XferConvert.TryDeserializeFromFileAsync<SampleData>(tempFile);
            Console.WriteLine($"  TryDeserializeFromFileAsync: Success={success}");

            // Test with non-existent file
            var (failureSuccess, _) = await XferConvert.TryDeserializeFromFileAsync<SampleData>("non-existent-file.xfer");
            Console.WriteLine($"  TryDeserializeFromFileAsync (non-existent): Success={failureSuccess}");

            Console.WriteLine("\n✓ Async file operations demo completed successfully!");

        } catch (Exception ex) {
            Console.WriteLine($"✗ Error during async file operations: {ex.Message}");
        } finally {
            // Cleanup
            if (File.Exists(tempFile)) {
                File.Delete(tempFile);
                Console.WriteLine("✓ Temporary file cleaned up");
            }
        }
    }
}

public class XferClient : IDisposable {
    private readonly HttpClient _httpClient;
    private bool _disposed = false;

    public XferClient(string baseUrl) {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xfer"));
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json", 0.8));
    }

    public async Task<T?> GetAsync<T>(string endpoint) {
        try {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content)) {
                return default;
            }

            // Use async deserialization
            using var reader = new StringReader(content);
            return await XferConvert.DeserializeAsync<T>(reader);
        }
        catch (HttpRequestException ex) {
            Console.WriteLine($"HTTP error during GET: {ex.Message}");
            return default;
        }
        catch (Exception ex) {
            Console.WriteLine($"Error during GET: {ex.Message}");
            return default;
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, T data) {
        try {
            // Use async serialization
            using var stringWriter = new StringWriter();
            await XferConvert.SerializeAsync(data, stringWriter, Formatting.Pretty);
            var xferContent = stringWriter.ToString();

            var content = new StringContent(xferContent, Encoding.UTF8, "application/xfer");

            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseContent)) {
                return default;
            }

            // Use async deserialization
            using var reader = new StringReader(responseContent);
            return await XferConvert.DeserializeAsync<T>(reader);
        }
        catch (HttpRequestException ex) {
            Console.WriteLine($"HTTP error during POST: {ex.Message}");
            return default;
        }
        catch (Exception ex) {
            Console.WriteLine($"Error during POST: {ex.Message}");
            return default;
        }
    }

    public async Task<string?> GetRawAsync(string endpoint) {
        try {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex) {
            Console.WriteLine($"Error during raw GET: {ex.Message}");
            return null;
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (!_disposed && disposing) {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}
