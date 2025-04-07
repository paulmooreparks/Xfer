using ParksComputing.Xfer.Lang.Attributes;

namespace ParksComputing.XferKit.Workspace.Models;

public class RequestDefinition {
    [XferProperty("name")]
    public string? Name { get; set; }
    [XferProperty("description")]
    public string? Description { get; set; }
    [XferProperty("endpoint")]
    public string? Endpoint { get; set; }
    [XferProperty("method")]
    public string? Method { get; set; }
    [XferProperty("arguments")]
    public Dictionary<string, Argument> Arguments { get; set; } = [];
    [XferProperty("headers")]
    public Dictionary<string, string> Headers { get; set; } = [];
    [XferProperty("cookies")]
    public Dictionary<string, string> Cookies { get; set; } = [];
    [XferProperty("parameters")]
    public List<string> Parameters { get; set; } = [];
    [XferProperty("payload")]
    public string? Payload { get; set; }
    [XferProperty("properties")]
    public Dictionary<string, object>? Properties { get; set; } = [];
    [XferProperty("preRequest")]
    public string? PreRequest { get; set; }
    [XferProperty("postResponse")]
    public string? PostResponse { get; set; }
    [XferProperty("response")]
    public ResponseDefinition Response { get; set; } = new ResponseDefinition();

    public void Merge(RequestDefinition parentRequest) {
        if (parentRequest is null) { 
            return;
        }

        Name ??= parentRequest.Name;
        Description ??= parentRequest.Description;
        Endpoint ??= parentRequest.Endpoint;
        Method ??= parentRequest.Method;
        Payload ??= parentRequest.Payload;
        PreRequest ??= parentRequest.PreRequest;
        PostResponse ??= parentRequest.PostResponse;
        Response ??= parentRequest.Response;

        // Merge headers (child values override parent)
        foreach (var kvp in parentRequest.Headers) {
            Headers.TryAdd(kvp.Key, kvp.Value);
        }

        // Merge parameters (child overrides parent if same key)
        var paramDict = new Dictionary<string, string?>();

        // Add parent parameters first
        foreach (var param in parentRequest.Parameters) {
            var split = param.Split('=', 2);
            var key = split[0];
            var value = split.Length > 1 ? split[1] : null;
            paramDict[key] = value;
        }

        // Then add/override with child parameters
        foreach (var param in Parameters) {
            var split = param.Split('=', 2);
            var key = split[0];
            var value = split.Length > 1 ? split[1] : null;
            paramDict[key] = value;
        }

        // Rebuild Parameters list
        Parameters = paramDict
            .Select(kvp => kvp.Value is not null ? $"{kvp.Key}={kvp.Value}" : kvp.Key)
            .ToList();
    }
}
