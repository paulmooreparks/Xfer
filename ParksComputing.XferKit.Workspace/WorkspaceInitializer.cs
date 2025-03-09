using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using ParksComputing.XferKit.Workspace.Services;
using ParksComputing.XferKit.Workspace.Services.Impl;

namespace ParksComputing.XferKit.Workspace;

public static class WorkspaceInitializer {
    public static ISettingsService InitializeWorkspace(IServiceCollection services) {
        ISettingsService settingsService = null;

        try {
            string homeDirectory = GetUserHomeDirectory();
            string xfDirectory = Path.Combine(homeDirectory, Constants.XferDirectoryName);

            if (!Directory.Exists(xfDirectory)) {
                Directory.CreateDirectory(xfDirectory);
            }

            string configFilePath = Path.Combine(xfDirectory, Constants.WorkspacesFileName);
            string storeFilePath = Path.Combine(xfDirectory, Constants.StoreFileName);
            string pluginDirectory = Path.Combine(xfDirectory, Constants.PackageDirName);
            string environmentFilePath = Path.Combine(xfDirectory, Constants.EnvironmentFileName);

            if (!Directory.Exists(pluginDirectory)) {
                Directory.CreateDirectory(pluginDirectory);
            }

            LoadEnvironmentVariables(environmentFilePath);

            settingsService = new Services.Impl.SettingsService {
                XferSettingsDirectory = xfDirectory,
                ConfigFilePath = configFilePath,
                PluginDirectory = pluginDirectory,
                StoreFilePath = storeFilePath,
                EnvironmentFilePath = environmentFilePath
            };
        }
        catch (Exception ex) {
            throw new Exception($"Error initializing workspace: {ex.Message}", ex);
        }

        return settingsService;
    }

    private static void LoadEnvironmentVariables(string environmentFilePath) {
        if (File.Exists(environmentFilePath)) {
            var lines = File.ReadAllLines(environmentFilePath);

            foreach (var line in lines) {
                var trimmedLine = line.Trim();

                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith('#')) {
                    continue;
                }

                var parts = trimmedLine.Split('=', 2);

                if (parts.Length == 2) {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim().Trim('"');
                    Environment.SetEnvironmentVariable(key, value);
                }
            }
        }
    }

    private static string GetUserHomeDirectory() {
        if (OperatingSystem.IsWindows()) {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()) {
            return Environment.GetEnvironmentVariable("HOME")
                ?? throw new InvalidOperationException("HOME environment variable is not set.");
        }
        else {
            throw new PlatformNotSupportedException("Unsupported operating system.");
        }
    }
}
