using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Attributes;

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
                BaseUrl = "https://httpbin.org/"
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

        try {
            var xfer = File.ReadAllText(WorkspaceFilePath, Encoding.UTF8);
            baseConfig = XferConvert.Deserialize<BaseConfig>(xfer);
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"Error loading workspace file '{WorkspaceFilePath}': {ex.Message}");
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

public class BaseConfig {
    public string? ActiveWorkspace { get; set; }
    public Dictionary<string, WorkspaceConfig>? Workspaces { get; set; }
}

public class WorkspaceConfig {
    public string? BaseUrl { get; set; }
}
