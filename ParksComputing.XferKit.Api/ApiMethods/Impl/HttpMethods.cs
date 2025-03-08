using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.XferKit.Http.Services;
using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Api.ApiMethods.Impl;
internal class HttpMethods : IHttpMethods {
    private readonly IHttpService _httpService;

    public HttpMethods(
        IHttpService httpService
        ) 
    {
        _httpService = httpService;
    }

    public string ResponseContent { get; protected set; } = string.Empty;
    public int StatusCode { get; protected set; } = 0;
    public System.Net.Http.Headers.HttpResponseHeaders? Headers { get; protected set; } = default;

    public async Task<HttpResponseMessage?> GetAsync(
        string baseUrl, 
        IEnumerable<string>? queryParameters, 
        IEnumerable<string>? headers
        ) 
    {
        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler() {
            CookieContainer = cookieContainer,
            UseCookies = true
        };

        var response = await _httpService.GetAsync(
            baseUrl, 
            queryParameters, 
            headers
            );

        if (response != null) {
            Headers = response.Headers;
            ResponseContent = await response.Content.ReadAsStringAsync();
            StatusCode = (int)response.StatusCode;
            // List<Cookie> responseCookies = cookieContainer.GetCookies(baseUri).Cast<Cookie>().ToList();
        }

        return response;
    }

    public async Task<HttpResponseMessage?> PostAsync(
        string baseUrl, 
        string payload, 
        IEnumerable<string>? headers
        ) 
    {
        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler() {
            CookieContainer = cookieContainer,
            UseCookies = true
        };

        var response = await _httpService.PostAsync(
            baseUrl,
            payload,
            headers
            );

        if (response != null) {
            Headers = response.Headers;
            ResponseContent = await response.Content.ReadAsStringAsync();
            StatusCode = (int)response.StatusCode;
            // List<Cookie> responseCookies = cookieContainer.GetCookies(baseUri).Cast<Cookie>().ToList();
        }

        return response;
    }

    public async Task<HttpResponseMessage?> PutAsync(string baseUrl, string endpoint, string payload, IEnumerable<string>? headers) {
        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler() {
            CookieContainer = cookieContainer,
            UseCookies = true
        };

        var response = await _httpService.PutAsync(
            baseUrl,
            endpoint,
            payload,
            headers
        );

        if (response != null) {
            Headers = response.Headers;
            ResponseContent = await response.Content.ReadAsStringAsync();
            StatusCode = (int)response.StatusCode;
            // List<Cookie> responseCookies = cookieContainer.GetCookies(baseUri).Cast<Cookie>().ToList();
        }

        return response;
    }

    public async Task<HttpResponseMessage?> DeleteAsync(string baseUrl, string endpoint, IEnumerable<string>? headers) {
        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler() {
            CookieContainer = cookieContainer,
            UseCookies = true
        };

        return await _httpService.DeleteAsync(
            baseUrl,
            endpoint,
            headers
        );
    }
}
