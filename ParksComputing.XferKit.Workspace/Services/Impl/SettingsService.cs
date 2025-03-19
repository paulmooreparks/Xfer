using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.XferKit.Workspace.Services.Impl;

internal class SettingsService : ISettingsService {
    public string? XferSettingsDirectory { get; set; }
    public string ConfigFilePath { get; set; } = string.Empty;
    public string? StoreFilePath { get; set; }
    public string? PluginDirectory { get; set; }
    public string? EnvironmentFilePath { get; set; }

    public SettingsService() {
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

        XferSettingsDirectory = xfDirectory;
        ConfigFilePath = configFilePath;
        StoreFilePath = storeFilePath;
        PluginDirectory = pluginDirectory;
        EnvironmentFilePath = environmentFilePath;
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
