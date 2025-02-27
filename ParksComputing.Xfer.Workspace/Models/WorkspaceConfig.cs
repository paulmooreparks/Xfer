using ParksComputing.Xfer.Lang.Attributes;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Workspace.Models;

public class WorkspaceConfig {
    public string? Extend { get; set; }
    public string? BaseUrl { get; set; }
    [XferProperty("Requests")]
    public Dictionary<string, RequestDefinition> Requests { get; set; } = [];

    internal void Merge(WorkspaceConfig? parentWorkspace) {
        if (parentWorkspace is null)
            return;

        // Merge BaseUrl if not set in current workspace
        BaseUrl ??= parentWorkspace.BaseUrl;

        // Merge Requests (combine existing with parent, prioritizing child values)
        foreach (var kvp in parentWorkspace.Requests) {
            if (!Requests.ContainsKey(kvp.Key)) {
                Requests[kvp.Key] = kvp.Value; // Inherit from parent
            }
            else {
                // Merge the request definition (headers, parameters, etc.)
                Requests[kvp.Key].Merge(kvp.Value);
            }
        }
    }
}

public class RequestDefinition {
    public string? Endpoint { get; set; }
    public string? Method { get; set; }
    public Dictionary<string, string> Headers { get; set; } = [];
    public List<string> Parameters { get; set; } = [];
    public string? Payload { get; set; }
    public string? PreRequest { get; set; }
    public string? PostRequest { get; set; }

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

        // Merge Headers (child values override parent)
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