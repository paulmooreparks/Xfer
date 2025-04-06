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
            if (!Headers.ContainsKey(kvp.Key)) {
                Headers[kvp.Key] = kvp.Value;
            }
        }

        // Merge parameters (avoid duplicates, prioritize child)
        var paramSet = new HashSet<string>(parentRequest.Parameters);
        paramSet.UnionWith(Parameters); // Child parameters take precedence
        Parameters = [.. paramSet];
    }
}
