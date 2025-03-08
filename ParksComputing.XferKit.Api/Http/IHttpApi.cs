using System.Net.Http.Headers;

namespace ParksComputing.XferKit.Api.Http;
public interface IHttpApi {
    HttpResponseHeaders? Headers { get; }
    string ResponseContent { get; }
    int StatusCode { get; }

    HttpResponseMessage? Get(string baseUrl, IEnumerable<string>? queryParameters, IEnumerable<string>? headers);
    Task<HttpResponseMessage?> GetAsync(string baseUrl, IEnumerable<string>? queryParameters, IEnumerable<string>? headers);
    HttpResponseMessage? Post(string baseUrl, string payload, IEnumerable<string>? headers);
    Task<HttpResponseMessage?> PostAsync(string baseUrl, string payload, IEnumerable<string>? headers);
    Task<HttpResponseMessage?> PutAsync(string baseUrl, string endpoint, string payload, IEnumerable<string>? headers);
    HttpResponseMessage? Put(string baseUrl, string endpoint, string payload, IEnumerable<string>? headers);
    HttpResponseMessage? Delete(string baseUrl, string endpoint, IEnumerable<string>? headers);
    Task<HttpResponseMessage?> DeleteAsync(string baseUrl, string endpoint, IEnumerable<string>? headers);
}