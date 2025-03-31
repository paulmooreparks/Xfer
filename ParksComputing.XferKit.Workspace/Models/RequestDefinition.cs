namespace ParksComputing.XferKit.Workspace.Models;

public class RequestDefinition {
    public string? Name { get; set; }
    public string? Endpoint { get; set; }
    public string? Method { get; set; }
    public Dictionary<string, string> Headers { get; set; } = [];
    public Dictionary<string, string> Cookies { get; set; } = [];
    public List<string> Parameters { get; set; } = [];
    public string? Payload { get; set; }
    public IDictionary<string, object>? Properties { get; set; }
    public string? PreRequest { get; set; }
    public string? PostResponse { get; set; }
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
        PostResponse ??= parentRequest.PostResponse;
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
