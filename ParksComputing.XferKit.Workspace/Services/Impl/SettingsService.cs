using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.XferKit.Workspace.Services.Impl;

public class SettingsService : ISettingsService {
    public string? XferSettingsDirectory { get; set; }
    public string ConfigFilePath { get; set; } = string.Empty;
    public string? StoreFilePath { get; set; }
    public string? PluginDirectory { get; set; }
    public string? EnvironmentFilePath { get; set; }
}
