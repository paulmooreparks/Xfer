﻿using System.Text;

using Cliffer;

using Microsoft.Extensions.DependencyInjection;

using ParksComputing.Xfer.Cli.Services;
using ParksComputing.Xfer.Workspace;
using ParksComputing.Xfer.Workspace.Services;
using ParksComputing.Xfer.Http;
using ParksComputing.Xfer.Http.Services;
using ParksComputing.Xfer.Cli.Services.Impl;

namespace ParksComputing.Xfer.Cli;

internal class Program {
    private static readonly string _configFilePath;
    private static readonly string _xfDirectory;
    private static readonly string _pluginDirectory;

    static Program() {
        try {
            // Get user's home directory in a cross-platform way
            string homeDirectory = GetUserHomeDirectory();

            // Define the .xf directory path
            _xfDirectory = Path.Combine(homeDirectory, Constants.XferDirectoryName);

            // Ensure the directory exists
            if (!Directory.Exists(_xfDirectory)) {
                Directory.CreateDirectory(_xfDirectory);
            }

            // Define the configuration file path
            _configFilePath = Path.Combine(_xfDirectory, Constants.ConfigFileName);
            _pluginDirectory = Path.Combine(_xfDirectory, Constants.PackageDirName);

            if (!Directory.Exists(_pluginDirectory)) {
                Directory.CreateDirectory(_pluginDirectory);
            }
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"Error initializing .xf directory: {ex.Message}");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Gets the user's home directory in a cross-platform way.
    /// </summary>
    private static string GetUserHomeDirectory() {
        if (OperatingSystem.IsWindows()) {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()) {
            return Environment.GetEnvironmentVariable("HOME") ?? throw new InvalidOperationException("HOME environment variable is not set.");
        }
        else {
            throw new PlatformNotSupportedException("Unsupported operating system.");
        }
    }

    static async Task<int> Main(string[] args) {
        var cli = new ClifferBuilder()
            .ConfigureServices(services => {
                services.AddSingleton<PersistenceService>();
                services.AddXferHttpServices();
                services.AddXferWorkspaceServices(_configFilePath);
                services.AddSingleton<CommandSplitter>();
                services.AddSingleton<PackageService>(provider => new PackageService(_pluginDirectory));
                services.AddSingleton<ScriptEngine>();
            })
            .Build();

        Cliffer.Macro.CustomMacroArgumentProcessor += CustomMacroArgumentProcessor;

        Utility.SetServiceProvider(cli.ServiceProvider);

        ClifferEventHandler.OnExit += () => {
            var persistenceService = Utility.GetService<PersistenceService>()!;
        };

        Console.OutputEncoding = Encoding.UTF8;

        return await cli.RunAsync(args);
    }

    private static string[] CustomMacroArgumentProcessor(string[] args) {
        for (int i = 0; i < args.Length; i++) {
            // Find the first instance of the baseurl option flag and its argument. 
            if (args[i] == "-b" || args[i] == "--baseurl") {
                // Index 'i' now points to the first occurrence.
                // Continue the loop with index 'j' starting at 'i + 2'.
                for (int j = i + 2; j < args.Length; ++j) {
                    // If there is a second instance of the baseurl option flag, 
                    // remove the first instance and its argument.
                    if (args[j] == "-b" || args[j] == "--baseurl") {
                        var newArgs = new List<string>();

                        // Copy all arguments to a new collection, except the 
                        // first and second occurrences.
                        for (int k = 0; k < args.Length; k++) {
                            if (k == i || k == i + 1) {
                                continue;
                            }

                            newArgs.Add(args[k]);
                        }

                        // We only expect to find another instance after the 
                        // first one, so early termination is okay.
                        return newArgs.ToArray();
                    }
                }
            }
        }

        return args;
    }
}
