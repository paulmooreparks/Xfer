using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;

using ParksComputing.XferKit.Api.ApiMethods;
using ParksComputing.XferKit.Workspace.Models;
using ParksComputing.XferKit.Workspace.Services;

// using Microsoft.ClearScript;

namespace ParksComputing.XferKit.Api;

public class XferKitApi : DynamicObject {
    private readonly Dictionary<string, object?> _properties = new();
    private readonly IHttpMethods _httpMethods;
    private readonly IWorkspaceService _workspaceService;

    public string ResponseContent { get; protected set; } = string.Empty;
    public int StatusCode { get; protected set; } = 0;
    public System.Net.Http.Headers.HttpResponseHeaders? Headers { get; protected set; } = default;

    public IEnumerable<string> WorkspaceList => _workspaceService.WorkspaceList;
    public string CurrentWorkspaceName => _workspaceService.CurrentWorkspaceName;

    public XferKitApi(
        IWorkspaceService workspaceService,
        IHttpMethods httpMethods
        ) 
    {
        _workspaceService = workspaceService;
        _httpMethods = httpMethods;
    }

    public void SetActiveWorkspace(string workspaceName) => _workspaceService.SetActiveWorkspace(workspaceName);

    public WorkspaceConfig ActiveWorkspace => _workspaceService.ActiveWorkspace;

    public override bool TryGetMember(GetMemberBinder binder, out object? result) {
        return _properties.TryGetValue(binder.Name, out result);
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value) {
        _properties[binder.Name] = value;
        return true;
    }

    public override IEnumerable<string> GetDynamicMemberNames() => _properties.Keys;

    public async Task<HttpResponseMessage?> GetAsync(
        string baseUrl,
        IEnumerable<string>? queryParameters,
        IEnumerable<string>? headers
        ) 
    { 
        var response = await _httpMethods.GetAsync(baseUrl, queryParameters, headers);

        if (response is not null) {             Headers = response.Headers;
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
        var response = await _httpMethods.PostAsync(baseUrl, payload, headers);

        if (response is not null) {
            Headers = response.Headers;
            ResponseContent = await response.Content.ReadAsStringAsync();
            StatusCode = (int)response.StatusCode;
            // List<Cookie> responseCookies = cookieContainer.GetCookies(baseUri).Cast<Cookie>().ToList();
        }

        return response;
    }

    public async Task<HttpResponseMessage?> PutAsync(
        string baseUrl, 
        string endpoint, 
        string payload, 
        IEnumerable<string>? headers
        ) 
    {
        var response = await _httpMethods.PutAsync(baseUrl, endpoint, payload, headers);

        if (response is not null) {
            Headers = response.Headers;
            ResponseContent = await response.Content.ReadAsStringAsync();
            StatusCode = (int)response.StatusCode;
            // List<Cookie> responseCookies = cookieContainer.GetCookies(baseUri).Cast<Cookie>().ToList();
        }

        return response;
    }

    public async Task<HttpResponseMessage?> DeleteAsync(
        string baseUrl,
        string endpoint,
        IEnumerable<string>? headers
        ) 
    {
        var response = await _httpMethods.DeleteAsync(baseUrl, endpoint, headers);

        if (response is not null) {
            Headers = response.Headers;
            ResponseContent = await response.Content.ReadAsStringAsync();
            StatusCode = (int)response.StatusCode;
            // List<Cookie> responseCookies = cookieContainer.GetCookies(baseUri).Cast<Cookie>().ToList();
        }

        return response;
    }
}
