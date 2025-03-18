namespace ParksComputing.XferKit.Workspace.Models;

public class ResponseDefinition {
    public int statusCode { get; set; }
    public string? body { get; set; }
    public System.Net.Http.Headers.HttpResponseHeaders? headers { get; set; } = default;
}