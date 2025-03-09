using System.Net.Http.Headers;

namespace ParksComputing.XferKit.Api.Http;

public interface IHttpApi {
    HttpResponseHeaders? headers { get; }
    string responseContent { get; }
    int statusCode { get; }

    HttpResponseMessage? get(string baseUrl, IEnumerable<string>? queryParameters, IEnumerable<string>? headers);
    Task<HttpResponseMessage?> getAsync(string baseUrl, IEnumerable<string>? queryParameters, IEnumerable<string>? headers);
    HttpResponseMessage? post(string baseUrl, string payload, IEnumerable<string>? headers);
    Task<HttpResponseMessage?> postAsync(string baseUrl, string payload, IEnumerable<string>? headers);
    HttpResponseMessage? put(string baseUrl, string endpoint, string payload, IEnumerable<string>? headers);
    Task<HttpResponseMessage?> putAsync(string baseUrl, string endpoint, string payload, IEnumerable<string>? headers);
    HttpResponseMessage? delete(string baseUrl, string endpoint, IEnumerable<string>? headers);
    Task<HttpResponseMessage?> deleteAsync(string baseUrl, string endpoint, IEnumerable<string>? headers);
}