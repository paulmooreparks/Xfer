using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang;

namespace ParksComputing.Xfer.Workspace.Services.Impl;

internal class WorkspaceService : IWorkspaceService
{
    public string? BaseUrl { get; set; }
    public string WorkspaceFilePath { get; protected set; }

    public WorkspaceService(string workspaceFilePath)
    {
        BaseUrl = string.Empty;
        WorkspaceFilePath = workspaceFilePath ?? throw new ArgumentNullException(nameof(workspaceFilePath));

        EnsureWorkspaceFileExists();
        LoadWorkspace();
    }

    private void EnsureWorkspaceFileExists() {
        if (!File.Exists(WorkspaceFilePath)) {
            var defaultConfig = new WorkspaceConfig {
                BaseUrl = ""  // Default empty BaseUrl
            };

            var xfer = XferConvert.Serialize(defaultConfig);

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
    private void LoadWorkspace() {
        try {
            var xfer = File.ReadAllText(WorkspaceFilePath, Encoding.UTF8);
            var config = XferConvert.Deserialize<WorkspaceConfig>(xfer);
            BaseUrl = config?.BaseUrl ?? string.Empty;
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"Error loading workspace file '{WorkspaceFilePath}': {ex.Message}");
            BaseUrl = string.Empty;
        }
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
            var config = new WorkspaceConfig { BaseUrl = BaseUrl };
            var xfer = XferConvert.Serialize(config);

            File.WriteAllText(WorkspaceFilePath, xfer, Encoding.UTF8);
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"Error saving workspace file '{WorkspaceFilePath}': {ex.Message}");
        }
    }

    private class WorkspaceConfig {
        public string? BaseUrl { get; set; }
    }
}
