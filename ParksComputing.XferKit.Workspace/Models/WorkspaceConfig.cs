using ParksComputing.Xfer.Lang.Attributes;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.XferKit.Workspace.Models;

public class WorkspaceConfig {
    [XferProperty("name")]
    public string? Name { get; set; }
    [XferProperty("description")]
    public string? Description { get; set; }
    [XferProperty("extend")]
    public string? Extend { get; set; }
    [XferProperty("base")]
    public WorkspaceConfig? Base { get; protected set; }
    [XferProperty("baseUrl")]
    public string? BaseUrl { get; set; }
    [XferProperty("initScript")]
    public string? InitScript { get; set; }
    [XferProperty("preRequest")]
    public string? PreRequest { get; set; }
    [XferProperty("postResponse")]
    public string? PostResponse { get; set; }
    [XferProperty("properties")]
    public Dictionary<string, object> Properties { get; set; } = [];
    [XferProperty("requests")]
    public Dictionary<string, RequestDefinition> Requests { get; set; } = [];
    [XferProperty("scripts")]
    public Dictionary<string, ScriptDefinition> Scripts { get; set; } = [];
    [XferProperty("macros")]
    public Dictionary<string, MacroDefinition> Macros { get; set; } = [];

    internal void Merge(WorkspaceConfig? parentWorkspace) {
        if (parentWorkspace is null) {
            return;
        }

        Name ??= parentWorkspace.Name;
        Description ??= parentWorkspace.Description;
        Extend ??= parentWorkspace.Extend;
        Base ??= parentWorkspace.Base;

        BaseUrl ??= parentWorkspace.BaseUrl;
        InitScript ??= parentWorkspace.InitScript;
        PreRequest ??= parentWorkspace.PreRequest;
        PostResponse ??= parentWorkspace.PostResponse;

        foreach (var kvp in parentWorkspace.Requests) {
            if (!Requests.ContainsKey(kvp.Key)) {
                Requests[kvp.Key] = kvp.Value; 
            }
            else {
                Requests[kvp.Key].Merge(kvp.Value);
            }
        }

        foreach (var kvp in parentWorkspace.Scripts) {
            if (!Scripts.ContainsKey(kvp.Key)) {
                Scripts[kvp.Key] = kvp.Value; 
            }
            else {
                Scripts[kvp.Key].Merge(kvp.Value);
            }
        }

        foreach (var kvp in parentWorkspace.Macros) {
            if (!Macros.ContainsKey(kvp.Key)) {
                Macros[kvp.Key] = kvp.Value; 
            }
            else {
                Macros[kvp.Key].Merge(kvp.Value);
            }
        }
    }
}
