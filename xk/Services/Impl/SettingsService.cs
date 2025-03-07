using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.XferKit.Cli.Services.Impl;

internal class SettingsService : ISettingsService {
    public string? XferSettingsDirectory { get; set; }
    public string? ConfigFilePath { get; set; }
    public string? StoreFilePath { get; set; }
    public string? PluginDirectory { get; set; }
    public string? EnvironmentFilePath { get; set; }
}
