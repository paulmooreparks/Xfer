using ParksComputing.Xfer.Lang.Attributes;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.XferKit.Workspace.Models;

public class WorkspaceConfig {
    public string? Name { get; set; }
    public string? Extend { get; set; }
    public WorkspaceConfig? Base { get; protected set; }
    public string? BaseUrl { get; set; }
    public dynamic? Properties { get; set; }
    public string? InitScript { get; set; }
    public string? PreRequest { get; set; }
    public string? PostRequest { get; set; }
    public Dictionary<string, RequestDefinition> Requests { get; set; } = [];

    internal void Merge(WorkspaceConfig? parentWorkspace) {
        if (parentWorkspace is null) {
            return;
        }

        Extend ??= parentWorkspace.Extend;
        Base ??= parentWorkspace.Base;

        BaseUrl ??= parentWorkspace.BaseUrl;
        InitScript ??= parentWorkspace.InitScript;
        PreRequest ??= parentWorkspace.PreRequest;
        PostRequest ??= parentWorkspace.PostRequest;
        Name ??= parentWorkspace.Name;

        foreach (var kvp in parentWorkspace.Requests) {
            if (!Requests.ContainsKey(kvp.Key)) {
                Requests[kvp.Key] = kvp.Value; 
            }
            else {
                Requests[kvp.Key].Merge(kvp.Value);
            }
        }
    }
}

public class RequestDefinition {
    public string? Name { get; set; }
    public string? Endpoint { get; set; }
    public string? Method { get; set; }
    public Dictionary<string, string> Headers { get; set; } = [];
    public Dictionary<string, string> Cookies { get; set; } = [];
    public List<string> Parameters { get; set; } = [];
    public string? Payload { get; set; }
    public dynamic? Properties { get; set; }
    public string? PreRequest { get; set; }
    public string? PostRequest { get; set; }
    public ResponseDefinition Response { get; set; } = new ResponseDefinition();

    public void Merge(RequestDefinition parentRequest) {
        if (parentRequest is null) { 
            return;
        }

        // If Endpoint or Method are missing, inherit from parent
        Endpoint ??= parentRequest.Endpoint;
        Method ??= parentRequest.Method;
        Payload ??= parentRequest.Payload;
        PreRequest ??= parentRequest.PreRequest;
        PostRequest ??= parentRequest.PostRequest;
        Response ??= parentRequest.Response;

        // Merge headers (child values override parent)
        foreach (var kvp in parentRequest.Headers) {
            if (!Headers.ContainsKey(kvp.Key)) {
                Headers[kvp.Key] = kvp.Value;
            }
        }

        // Merge Parameters (avoid duplicates, prioritize child)
        var paramSet = new HashSet<string>(parentRequest.Parameters);
        paramSet.UnionWith(Parameters); // Child parameters take precedence
        Parameters = paramSet.ToList();
    }
}

public class ResponseDefinition {
    public int statusCode { get; set; }
    public string? body { get; set; }
    public System.Net.Http.Headers.HttpResponseHeaders? headers { get; set; } = default;
}