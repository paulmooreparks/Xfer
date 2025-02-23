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

    public HttpService() {
    }

    public async Task<string> GetAsync(HttpClient httpClient, string baseUrl, IEnumerable<string> queryParameters, string? accessToken = null) {
        // if (accessToken is null) {
        //     accessToken = _config.GetConfigValue("AccessToken");
        // }


        // if (string.IsNullOrEmpty(accessToken)) {
        //     throw new InvalidOperationException("Access token not found. Please log in.");
        // }

        // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        // httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var query = string.Join("&", queryParameters.Select(param => {
            if (param.Contains("=")) {
                var parts = param.Split(new[] { '=' }, 2); // Splitting only on the first occurrence
                if (parts.Length == 2) {
                    // Key=value pair, escape separately
                    return $"{Uri.EscapeDataString(parts[0])}={Uri.EscapeDataString(parts[1])}";
                }
                else {
                    // Shouldn't reach here due to the check, but just in case
                    return Uri.EscapeDataString(param);
                }
            }
            else {
                // Not a key=value pair, escape the whole parameter
                return Uri.EscapeDataString(param);
            }
        }));

        if (!string.IsNullOrEmpty(query)) {
            baseUrl = string.Join ("?", baseUrl, query);
        }

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) || string.IsNullOrWhiteSpace(baseUri.Scheme)) {
            throw new HttpRequestException($"Error: Invalid base URL: {baseUrl}");
        }

        var response = await httpClient.GetAsync(baseUrl);

        if (!response.IsSuccessStatusCode) {
            throw new HttpRequestException($"{response.ReasonPhrase} calling GET at {baseUrl}", null, statusCode: response.StatusCode);
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        // var jsonObject = JsonConvert.DeserializeObject(responseContent);
        // var formattedJson = JsonConvert.SerializeObject(jsonObject, Newtonsoft.Json.Formatting.Indented);
        // return formattedJson;
        return responseContent;
    }

    public async Task<string> PostAsync(HttpClient httpClient, string baseUrl, string payload, string? accessToken = null) {
        //if (accessToken is null) {
        //    accessToken = _config.GetConfigValue("AccessToken");
        //}

        //if (string.IsNullOrEmpty(accessToken)) {
        //    throw new InvalidOperationException("Access token not found. Please log in.");
        //}

        //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        //httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) || string.IsNullOrWhiteSpace(baseUri.Scheme)) {
            throw new HttpRequestException($"Error: Invalid base URL: {baseUrl}");
        }

        var response = await httpClient.PostAsync(baseUrl, new StringContent(payload, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode) {
            throw new HttpRequestException($"{response.ReasonPhrase} calling POST at {baseUrl}", null, statusCode: response.StatusCode);
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        // var jsonObject = JsonConvert.DeserializeObject(responseContent);
        // var formattedJson = JsonConvert.SerializeObject(jsonObject, Newtonsoft.Json.Formatting.Indented);
        return responseContent;
    }

    public async Task<string> PutAsync(HttpClient httpClient, string baseUrl, string endpoint, string payload, string? accessToken = null) {
        //if (accessToken is null) {
        //    accessToken = _config.GetConfigValue("AccessToken");
        //}

        //if (string.IsNullOrEmpty(accessToken)) {
        //    throw new InvalidOperationException("Access token not found. Please log in.");
        //}

        //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        //httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) || string.IsNullOrWhiteSpace(baseUri.Scheme)) {
            throw new HttpRequestException($"Error: Invalid base URL: {baseUrl}");
        }

        var fullUrl = new Uri(baseUri, endpoint).ToString();
        var response = await httpClient.PutAsync(fullUrl, new StringContent(payload, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode) {
            throw new HttpRequestException($"{response.ReasonPhrase} calling PUT at {fullUrl}", null, statusCode: response.StatusCode);
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        // var jsonObject = JsonConvert.DeserializeObject(responseContent);
        // var formattedJson = JsonConvert.SerializeObject(jsonObject, Newtonsoft.Json.Formatting.Indented);
        return responseContent;
    }

    public async Task<string> DeleteAsync(HttpClient httpClient, string baseUrl, string endpoint, string? accessToken = null) {
        //if (accessToken is null) {
        //    accessToken = _config.GetConfigValue("AccessToken");
        //}

        //if (string.IsNullOrEmpty(accessToken)) {
        //    throw new InvalidOperationException("Access token not found. Please log in.");
        //}

        //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        //httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) || string.IsNullOrWhiteSpace(baseUri.Scheme)) {
            throw new HttpRequestException($"Error: Invalid base URL: {baseUrl}");
        }

        var fullUrl = new Uri(baseUri, endpoint).ToString();
        var response = await httpClient.DeleteAsync(fullUrl);

        if (!response.IsSuccessStatusCode) {
            throw new HttpRequestException($"{response.ReasonPhrase} calling DELETE at {fullUrl}", null, statusCode: response.StatusCode);
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        // var jsonObject = JsonConvert.DeserializeObject(responseContent);
        // var formattedJson = JsonConvert.SerializeObject(jsonObject, Newtonsoft.Json.Formatting.Indented);
        return responseContent;
    }
}
