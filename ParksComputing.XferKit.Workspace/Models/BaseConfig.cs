using ParksComputing.Xfer.Lang.Attributes;

namespace ParksComputing.XferKit.Workspace.Models;

public class BaseConfig
{
    [XferProperty("activeWorkspace")]
    public string? ActiveWorkspace { get; set; }
    [XferProperty("initScript")]
    public string? InitScript { get; set; }
    [XferProperty("preRequest")]
    public string? PreRequest { get; set; }
    [XferProperty("postResponse")]
    public string? PostResponse { get; set; }
    [XferProperty("properties")]
    public Dictionary<string, object> Properties { get; set; } = [];
    [XferProperty("workspaces")]
    public Dictionary<string, WorkspaceConfig> Workspaces { get; set; } = [];
    [XferProperty("macros")]
    public Dictionary<string, MacroDefinition> Macros { get; set; } = [];
    [XferProperty("scripts")]
    public Dictionary<string, ScriptDefinition> Scripts { get; set; } = [];
    [XferProperty("assemblies")]
    public string[]? Assemblies { get; set; }
}
