using System.Text;

using Cliffer;

using Microsoft.Extensions.DependencyInjection;

using ParksComputing.Xfer.Cli.Services;

namespace ParksComputing.Xfer.Cli;

internal class Program {
    private static readonly string _configFilePath;
    private static readonly string _xfercDirectory;

    static Program() {
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _xfercDirectory = Path.Combine(homeDirectory, Constants.XferDirectoryName);

        if (!Directory.Exists(_xfercDirectory)) {
            Directory.CreateDirectory(_xfercDirectory);
        }

        _configFilePath = Path.Combine(_xfercDirectory, Constants.ConfigFileName);
    }

    static async Task<int> Main(string[] args) {
        var cli = new ClifferBuilder()
            .ConfigureAppConfiguration((configurationBuilder) => {
                // configurationBuilder.AddJsonFile(_configFilePath, true);
            })
            .ConfigureServices(services => {
                services.AddSingleton<PersistenceService>();
                services.AddSingleton<IHttpService, HttpService>();
                services.AddSingleton<IWorkspaceService, WorkspaceService>();
                services.AddSingleton<CommandSplitter>();
            })
            .Build();

        Utility.SetServiceProvider(cli.ServiceProvider);

        ClifferEventHandler.OnExit += () => {
            var persistenceService = Utility.GetService<PersistenceService>()!;
        };

        Console.OutputEncoding = Encoding.UTF8;

        return await cli.RunAsync(args);
    }
}
