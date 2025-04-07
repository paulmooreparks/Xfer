﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Attributes;
using ParksComputing.XferKit.Diagnostics.Services;
using ParksComputing.XferKit.Workspace.Models;

namespace ParksComputing.XferKit.Workspace.Services.Impl;

internal class WorkspaceService : IWorkspaceService
{
    private readonly ISettingsService _settingsService;
    private readonly IAppDiagnostics<WorkspaceService> _diags;

    public BaseConfig BaseConfig { get; protected set; }
    public WorkspaceConfig ActiveWorkspace { get; protected set; }
    public string WorkspaceFilePath { get; protected set; }
    public string CurrentWorkspaceName => BaseConfig?.ActiveWorkspace ?? string.Empty;

    private readonly string _packageDirectory;

    public IEnumerable<string> WorkspaceList {
        get {
            if (BaseConfig is not null && BaseConfig.Workspaces is not null) { 
                return BaseConfig.Workspaces.Keys;
            }

            return new List<string>();
        }
    }

    public WorkspaceService(
        ISettingsService settingsService,
        IAppDiagnostics<WorkspaceService> appDiagnostics
        )
    {
        WorkspaceInitializer.InitializeWorkspace(settingsService);
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _diags = appDiagnostics ?? throw new ArgumentNullException(nameof(appDiagnostics));

        ActiveWorkspace = new WorkspaceConfig();
        WorkspaceFilePath = _settingsService.ConfigFilePath;
        _packageDirectory = _settingsService.PluginDirectory ?? string.Empty;

        EnsureWorkspaceFileExists();
        BaseConfig = LoadWorkspace();
        LoadConfiguredAssemblies();
        SetActiveWorkspace(BaseConfig.ActiveWorkspace ?? string.Empty);

    }

    public void SetActiveWorkspace(string workspaceName) {
        if (!string.IsNullOrEmpty(workspaceName)) {
            if (BaseConfig.Workspaces is not null) {
                if (string.Equals(workspaceName, "/")) {
                    ActiveWorkspace = new WorkspaceConfig();
                    BaseConfig.ActiveWorkspace = string.Empty;
                }
                else if (BaseConfig.Workspaces.ContainsKey(workspaceName)) {
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
                throw new Exception($"Error creating workspace file '{WorkspaceFilePath}': {ex.Message}", ex);
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
                // baseConfig.activeWorkspace = "default";
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

#if false
    public void LoadWorkspace(string workspaceFilePath) {
        if (!File.Exists(workspaceFilePath)) {
            throw new Exception($"Error: Workspace file '{workspaceFilePath}' not found.");
        }

        WorkspaceFilePath = workspaceFilePath;
        BaseConfig = LoadWorkspace();
    }
#endif

    public IEnumerable<Assembly> LoadConfiguredAssemblies() {
        var loadedAssemblies = new List<Assembly>();

        var assemblyNames = BaseConfig.Assemblies;
        if (assemblyNames is null)
            return loadedAssemblies;

        foreach (var name in assemblyNames) {
            var path = Path.IsPathRooted(name)
                ? name
                : Path.Combine(_packageDirectory, name);

            if (File.Exists(path)) {
                try {
                    var assembly = Assembly.LoadFrom(path);
                    loadedAssemblies.Add(assembly);
                }
                catch (Exception ex) {
                    _diags.Emit(
                        nameof(IWorkspaceService), 
                        new { 
                            Message = $"Failed to load assembly {path}: {ex.Message}",
                            ex 
                        }
                    );
                }
            }
            else {
                _diags.Emit(
                    nameof(IWorkspaceService),
                    new {
                        Message = $"Assembly not found: {path}"
                    }
                );
            }
        }

        return loadedAssemblies;
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
            throw new Exception($"Error saving workspace file '{WorkspaceFilePath}': {ex.Message}", ex);
        }
    }
}
