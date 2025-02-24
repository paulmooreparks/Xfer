using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// using Newtonsoft.Json;
// using System.CommandLine;
// using Cliffer;


namespace ParksComputing.Xfer.Http.Services.Impl;

public class HttpService : IHttpService {
    private readonly HttpClient _httpClient;

    public HttpService(HttpClient httpClient) {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    private static void AddHeaders(HttpRequestMessage request, IEnumerable<string> headers) {
        if (headers is not null) {
            foreach (var header in headers) {
                var parts = header.Split(new[] { ':' }, 2);

                if (parts.Length == 2) {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    if (!request.Headers.TryAddWithoutValidation(key, value)) {
                        if (request.Content == null) {
                            request.Content = new StringContent("");
                        }
                        request.Content.Headers.TryAddWithoutValidation(key, value);
                    }
                }
            }
        }
    }

    public async Task<HttpResponseMessage> GetAsync(string baseUrl, IEnumerable<string> queryParameters, IEnumerable<string> headers) {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) || string.IsNullOrWhiteSpace(baseUri.Scheme)) {
            throw new HttpRequestException($"Error: Invalid base URL: {baseUrl}");
        }

        var uriBuilder = new UriBuilder(baseUri);
        var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

        if (queryParameters is not null) {
            foreach (var param in queryParameters) {
                var parts = param.Split(new[] { '=' }, 2);
                if (parts.Length == 2) {
                    query[parts[0]] = parts[1];
                }
                else {
                    query[param] = string.Empty;
                }
            }
        }

        uriBuilder.Query = query.ToString();
        var finalUrl = uriBuilder.ToString();

        using var request = new HttpRequestMessage(HttpMethod.Get, finalUrl);
        AddHeaders(request, headers);

        return await _httpClient.SendAsync(request);
    }

    public async Task<HttpResponseMessage> PostAsync(string baseUrl, string payload, IEnumerable<string> headers) {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) || string.IsNullOrWhiteSpace(baseUri.Scheme)) {
            throw new HttpRequestException($"Error: Invalid base URL: {baseUrl}");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, baseUrl) {
            Content = !string.IsNullOrEmpty(payload)
            ? new StringContent(payload, Encoding.UTF8, "application/json")
            : null
        };

        AddHeaders(request, headers);

        var response = await _httpClient.SendAsync(request);
        return response;
    }

    public async Task<HttpResponseMessage> PutAsync(string baseUrl, string endpoint, string payload, IEnumerable<string> headers) {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) || string.IsNullOrWhiteSpace(baseUri.Scheme)) {
            throw new HttpRequestException($"Error: Invalid base URL: {baseUrl}");
        }

        var fullUrl = new UriBuilder(baseUri) { Path = endpoint }.ToString();

        using var request = new HttpRequestMessage(HttpMethod.Put, fullUrl) {
            Content = !string.IsNullOrEmpty(payload)
                ? new StringContent(payload, Encoding.UTF8, "application/json")
                : null
        };

        AddHeaders(request, headers);

        return await _httpClient.SendAsync(request);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string baseUrl, string endpoint, IEnumerable<string> headers) {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) || string.IsNullOrWhiteSpace(baseUri.Scheme)) {
            throw new HttpRequestException($"Error: Invalid base URL: {baseUrl}");
        }

        var fullUrl = new UriBuilder(baseUri) { Path = endpoint }.ToString();

        using var request = new HttpRequestMessage(HttpMethod.Delete, fullUrl);
        AddHeaders(request, headers);

        return await _httpClient.SendAsync(request);
    }
}
