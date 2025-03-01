﻿namespace ParksComputing.Xfer.Cli.Services;

internal interface ISettingsService
{
    string? XferSettingsDirectory { get; set; }
    string? ConfigFilePath { get; set; }
    string? StoreFilePath { get; set; }
    string? PluginDirectory { get; set; }
    string? EnvironmentFilePath { get; set; }
}