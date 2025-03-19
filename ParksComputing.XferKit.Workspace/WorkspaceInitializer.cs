using Microsoft.Extensions.DependencyInjection;
using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Workspace;

public static class WorkspaceInitializer {
    public static void InitializeWorkspace(ISettingsService settingsService) {
        try {
            LoadEnvironmentVariables(settingsService.EnvironmentFilePath);
        }
        catch (Exception ex) {
            throw new Exception($"Error initializing workspace: {ex.Message}", ex);
        }
    }

    private static void LoadEnvironmentVariables(string? environmentFilePath) {
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
}
