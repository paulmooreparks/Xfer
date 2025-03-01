using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Attributes;
using ParksComputing.Xfer.Workspace.Models;

namespace ParksComputing.Xfer.Workspace.Services.Impl;

internal class WorkspaceService : IWorkspaceService
{
    public BaseConfig BaseConfig { get; protected set; }
    public WorkspaceConfig ActiveWorkspace { get; protected set; }
    public string WorkspaceFilePath { get; protected set; }
    public string CurrentWorkspaceName => BaseConfig?.ActiveWorkspace ?? string.Empty; 

    public IEnumerable<string> WorkspaceList {
        get {
            if (BaseConfig is not null && BaseConfig.Workspaces is not null) { 
                return BaseConfig.Workspaces.Keys;
            }

            return new List<string>();
        }
    }

    public WorkspaceService(string workspaceFilePath)
    {
        ActiveWorkspace = new WorkspaceConfig();
        WorkspaceFilePath = workspaceFilePath ?? throw new ArgumentNullException(nameof(workspaceFilePath));

        EnsureWorkspaceFileExists();
        BaseConfig = LoadWorkspace();
        SetActiveWorkspace(BaseConfig.ActiveWorkspace ?? string.Empty);
    }

    public void SetActiveWorkspace(string workspaceName) {
        if (!string.IsNullOrEmpty(workspaceName)) {
            if (BaseConfig.Workspaces is not null) {
                if (BaseConfig.Workspaces.ContainsKey(workspaceName)) {
                    ActiveWorkspace = BaseConfig.Workspaces[workspaceName];
                    BaseConfig.ActiveWorkspace = workspaceName;
                }
            }
        }
    }

    private void EnsureWorkspaceFileExists() {
        if (!File.Exists(WorkspaceFilePath)) {
            var xferDocument = new XferDocument();

            var dict = new Dictionary<string, WorkspaceConfig>();

            var defaultConfig = new WorkspaceConfig {
                Name = "default",
                BaseUrl = string.Empty,
            };

            dict.Add("default", defaultConfig);

            var baseConfig = new {
                Workspaces = dict
            };

            var xfer = XferConvert.Serialize(baseConfig, Formatting.Indented | Formatting.Spaced);

            try {
                File.WriteAllText(WorkspaceFilePath, xfer, Encoding.UTF8);
            }
            catch (Exception ex) {
                Console.Error.WriteLine($"Error creating workspace file '{WorkspaceFilePath}': {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Loads configuration from the file.
    /// </summary>
    private BaseConfig LoadWorkspace() {
        var baseConfig = new BaseConfig();

            var xfer = File.ReadAllText(WorkspaceFilePath, Encoding.UTF8);
            baseConfig = XferConvert.Deserialize<BaseConfig>(xfer);

            if (baseConfig.Workspaces is null) {
                baseConfig.Workspaces = new Dictionary<string, WorkspaceConfig>();
            }

            if (baseConfig.ActiveWorkspace is null) {
                baseConfig.ActiveWorkspace = "default";
            }

            foreach (var workspaceKvp in baseConfig.Workspaces) {
                var workspace = workspaceKvp.Value;

                if (workspace is not null) {
                    workspace.Name = workspaceKvp.Key;

                    foreach (var reqKvp in workspace.Requests) {
                        reqKvp.Value.Name = reqKvp.Key;
                    }

                    if (workspace.Extend is not null) {
                        if (baseConfig.Workspaces.TryGetValue(workspace.Extend, out var parentWorkspace)) {
                            workspace.Merge(parentWorkspace);
                        }
                    }
                }
            }

        return baseConfig;
    }

    public void LoadWorkspace(string workspaceFilePath) {
        if (!File.Exists(workspaceFilePath)) {
            Console.Error.WriteLine($"Error: Workspace file '{workspaceFilePath}' not found.");
            return;
        }

        WorkspaceFilePath = workspaceFilePath;
        LoadWorkspace();
    }

    /// <summary>
    /// Saves the current settings back to the configuration file.
    /// </summary>
    public void SaveConfig() {
        try {
            var xfer = XferConvert.Serialize(BaseConfig, Formatting.Indented | Formatting.Spaced);

            File.WriteAllText(WorkspaceFilePath, xfer, Encoding.UTF8);
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"Error saving workspace file '{WorkspaceFilePath}': {ex.Message}");
        }
    }
}
