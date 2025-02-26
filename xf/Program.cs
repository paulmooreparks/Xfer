using System.Text;

using Cliffer;

using Microsoft.Extensions.DependencyInjection;

using ParksComputing.Xfer.Cli.Services;
using ParksComputing.Xfer.Workspace;
using ParksComputing.Xfer.Workspace.Services;
using ParksComputing.Xfer.Http;
using ParksComputing.Xfer.Http.Services;

namespace ParksComputing.Xfer.Cli;

internal class Program {
    private static readonly string _configFilePath;
    private static readonly string _xfDirectory;

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
            })
            .Build();

        Cliffer.Macro.CustomMacroArgumentProcessor += (args) => {
            int? removeIndex = null;
            int? tmpIndex = null;

            for (int i = 0; i < args.Length; i++) {
                if (args[i] == "-b" || args[i] == "--baseurl") {
                    if (tmpIndex.HasValue) {
                        removeIndex = tmpIndex;
                        break;
                    }
                    else {
                        tmpIndex = i;
                    }
                }
            }

            if (removeIndex.HasValue) {
                var newArgs = new List<string>();

                for (int i = 0; i < args.Length; i++) {
                    if (i == removeIndex.Value || i == removeIndex.Value + 1) {
                        continue;
                    }

                    newArgs.Add(args[i]);
                }

                return newArgs.ToArray();
            }

            return args;
        };

        Utility.SetServiceProvider(cli.ServiceProvider);

        ClifferEventHandler.OnExit += () => {
            var persistenceService = Utility.GetService<PersistenceService>()!;
        };

        Console.OutputEncoding = Encoding.UTF8;

        return await cli.RunAsync(args);
    }
}
