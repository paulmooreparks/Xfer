using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Http.Services;

public interface IHttpService {
    Task<string> GetAsync(HttpClient httpClient, string baseUrl, IEnumerable<string> queryParameters, string? accessToken = null);
    Task<string> PostAsync(HttpClient httpClient, string baseUrl, string payload, string? accessToken = null);
    Task<string> PutAsync(HttpClient httpClient, string baseUrl, string endpoint, string payload, string? accessToken = null);
    Task<string> DeleteAsync(HttpClient httpClient, string baseUrl, string endpoint, string? accessToken = null);
}
