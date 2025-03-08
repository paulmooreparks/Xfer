using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.XferKit.Api.ApiMethods;
public interface IHttpMethods {
    string ResponseContent { get; }
    int StatusCode { get; }
    System.Net.Http.Headers.HttpResponseHeaders? Headers { get; }

    Task<HttpResponseMessage?> GetAsync(string baseUrl, IEnumerable<string>? queryParameters, IEnumerable<string>? headers);
    Task<HttpResponseMessage?> PostAsync(string baseUrl, string payload, IEnumerable<string>? headers);
    Task<HttpResponseMessage?> PutAsync(string baseUrl, string endpoint, string payload, IEnumerable<string>? headers);
    Task<HttpResponseMessage?> DeleteAsync(string baseUrl, string endpoint, IEnumerable<string>? headers);
}
