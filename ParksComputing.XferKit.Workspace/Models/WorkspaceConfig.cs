using ParksComputing.Xfer.Lang.Attributes;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.XferKit.Workspace.Models;

public class WorkspaceConfig {
    public string? Name { get; set; }
    public string? Extend { get; set; }
    public WorkspaceConfig? Base { get; protected set; }
    public string? BaseUrl { get; set; }
    public dynamic? Properties { get; set; }
    public string? InitScript { get; set; }
    public string? PreRequest { get; set; }
    public string? PostRequest { get; set; }
    public Dictionary<string, RequestDefinition> Requests { get; set; } = [];

    internal void Merge(WorkspaceConfig? parentWorkspace) {
        if (parentWorkspace is null) {
            return;
        }

        Extend ??= parentWorkspace.Extend;
        Base ??= parentWorkspace.Base;

        BaseUrl ??= parentWorkspace.BaseUrl;
        InitScript ??= parentWorkspace.InitScript;
        PreRequest ??= parentWorkspace.PreRequest;
        PostRequest ??= parentWorkspace.PostRequest;
        Name ??= parentWorkspace.Name;

        foreach (var kvp in parentWorkspace.Requests) {
            if (!Requests.ContainsKey(kvp.Key)) {
                Requests[kvp.Key] = kvp.Value; 
            }
            else {
                Requests[kvp.Key].Merge(kvp.Value);
            }
        }
    }
}
