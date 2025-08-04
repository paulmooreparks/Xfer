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

        // Serialize locally
        var serialized = XferConvert.Serialize(testData, Formatting.Pretty);
        Console.WriteLine($"Local serialization:\n{serialized}");

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

            return XferConvert.Deserialize<T>(content);
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
            var xferContent = XferConvert.Serialize(data, Formatting.Pretty);
            var content = new StringContent(xferContent, Encoding.UTF8, "application/xfer");
            
            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseContent)) {
                return default;
            }

            return XferConvert.Deserialize<T>(responseContent);
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
