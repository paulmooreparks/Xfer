using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.XferKit.Http.Services;

public interface IHttpService {
    HttpResponseMessage Get(string baseUrl, IEnumerable<string>? queryParameters, IEnumerable<string>? headers);
    Task<HttpResponseMessage> GetAsync(string baseUrl, IEnumerable<string>? queryParameters, IEnumerable<string>? headers);
    HttpResponseMessage Post(string baseUrl, string? payload, IEnumerable<string>? headers);
    Task<HttpResponseMessage> PostAsync(string baseUrl, string? payload, IEnumerable<string>? headers);
    Task<HttpResponseMessage> PutAsync(string baseUrl, string endpoint, string? payload, IEnumerable<string>? headers);
    Task<HttpResponseMessage> DeleteAsync(string baseUrl, string endpoint, IEnumerable<string>? headers);
}
