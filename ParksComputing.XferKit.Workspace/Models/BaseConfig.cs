namespace ParksComputing.XferKit.Workspace.Models;

public class BaseConfig
{
    public string? ActiveWorkspace { get; set; }
    public string? InitScript { get; set; }
    public string? PreRequest { get; set; }
    public string? PostRequest { get; set; }
    public IDictionary<string, object>? Properties { get; set; }
    public Dictionary<string, WorkspaceConfig>? Workspaces { get; set; }
    public string[]? Assemblies { get; set; }
}
