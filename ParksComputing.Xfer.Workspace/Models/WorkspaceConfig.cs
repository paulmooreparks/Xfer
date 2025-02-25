using ParksComputing.Xfer.Lang.Attributes;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Workspace.Models;

public class WorkspaceConfig {
    public string? BaseUrl { get; set; }
    [XferProperty("Requests")]
    public Dictionary<string, RequestDefinition> RequestDefinitions { get; set; } = [];
}

public class RequestDefinition {
    public string? Endpoint { get; set; }
    public string? Method { get; set; }
    public Dictionary<string, string> Headers { get; set; } = [];
    public List<string> Parameters { get; set; } = [];
    public string? Payload { get; set; }
}