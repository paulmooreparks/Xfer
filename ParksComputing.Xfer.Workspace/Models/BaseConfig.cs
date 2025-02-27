namespace ParksComputing.Xfer.Workspace.Models;

public class BaseConfig
{
    public string? ActiveWorkspace { get; set; }
    public string? InitScript { get; set; }
    public Dictionary<string, WorkspaceConfig>? Workspaces { get; set; }
}
