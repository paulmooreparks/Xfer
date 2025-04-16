using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;
using Xfer.Data;

using System.Net.Http.Headers;
using System.Text;

namespace Xfer.Client;


internal class Program {
    static async Task Main(string[] args) {
        var client = new XferClient("https://localhost:7021");

        // GET request
        var sampleData = await client.GetAsync<SampleData>("api/sampledata");
        Console.Write($@"
Name: {sampleData.Name}
Age: {sampleData.Age}
TestEmum: {sampleData.TestEnum}
TimeOnly: {sampleData.TimeOnly}
TimeSpan: {sampleData.TimeSpan}
DateTime: {sampleData.DateTime}
");

        // POST request
        sampleData = await client.PostAsync("api/sampledata", new SampleData { 
            Name = "Bob", 
            Age = 40,
            TimeSpan = new TimeSpan(90, 11, 43, 56),
            TimeOnly = new TimeOnly(23, 45, 56),
            TestEnum = TestEnum.Spaced, 
            DateTime = DateTime.Now 
        });

        Console.Write($@"
Name: {sampleData.Name}
Age: {sampleData.Age}
TestEmum: {sampleData.TestEnum}
TimeOnly: {sampleData.TimeOnly}
TimeSpan: {sampleData.TimeSpan}
DateTime: {sampleData.DateTime}
");
    }
}

public class XferClient {
    private readonly HttpClient _httpClient;

    public XferClient(string baseUrl) {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xfer"));
    }

    public async Task<T> GetAsync<T>(string endpoint) where T : new() {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return XferConvert.Deserialize<T>(content);
    }

    public async Task<T> PostAsync<T>(string endpoint, T data) where T : new() {
        var xferContent = XferConvert.Serialize(data!);
        var content = new StringContent(xferContent, Encoding.UTF8, "application/xfer");
        var response = await _httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return XferConvert.Deserialize<T>(responseContent);
    }
}
