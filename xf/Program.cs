using System.Text;

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
    private static readonly string _storeFilePath;
    private static readonly string _environmentFilePath;

    static Program() {
        try {
            string homeDirectory = GetUserHomeDirectory();

            _xfDirectory = Path.Combine(homeDirectory, Constants.XferDirectoryName);

            if (!Directory.Exists(_xfDirectory)) {
                Directory.CreateDirectory(_xfDirectory);
            }

            _configFilePath = Path.Combine(_xfDirectory, Constants.WorkspacesFileName);
            _storeFilePath = Path.Combine(_xfDirectory, Constants.StoreFileName);
            _pluginDirectory = Path.Combine(_xfDirectory, Constants.PackageDirName);
            _environmentFilePath = Path.Combine(_xfDirectory, Constants.EnvironmentFileName);

            if (!Directory.Exists(_pluginDirectory)) {
                Directory.CreateDirectory(_pluginDirectory);
            }

            LoadEnvironmentVariables();
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"Error initializing .xf directory: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static void LoadEnvironmentVariables() {
        if (File.Exists(_environmentFilePath)) {
            var lines = File.ReadAllLines(_environmentFilePath);

            foreach (var line in lines) {
                var trimmedLine = line.Trim();

                // Skip comments and empty lines
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
            return Environment.GetEnvironmentVariable("HOME") ?? throw new InvalidOperationException("HOME environment variable is not set.");
        }
        else {
            // Eh... let's do something about this
            throw new PlatformNotSupportedException("Unsupported operating system.");
        }
    }

    static async Task<int> Main(string[] args) {
        var cli = new ClifferBuilder()
            .ConfigureServices(services => {
                services.AddSingleton<PersistenceService>();
                services.AddXferHttpServices();
                services.AddXferWorkspaceServices(_configFilePath);
                services.AddSingleton<ISettingsService, SettingsService>(
                    provider => new SettingsService { 
                        XferSettingsDirectory = _xfDirectory, 
                        ConfigFilePath = _configFilePath, 
                        PluginDirectory = _pluginDirectory, 
                        StoreFilePath = _storeFilePath,
                        EnvironmentFilePath = _environmentFilePath
                    }
                );
                services.AddSingleton<ICommandSplitter, CommandSplitter>();
                services.AddSingleton<IPackageService, PackageService>(provider => new PackageService(_pluginDirectory));
                services.AddSingleton<IScriptEngine, ScriptEngine>();
                services.AddSingleton<IStoreService, StoreService>(provider => new StoreService(_storeFilePath));
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
