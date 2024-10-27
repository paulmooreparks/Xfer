using Cliffer;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ParksComputing.Xfer.Services;
using ParksComputing.Xfer.Models;
using ParksComputing.Xferc.Services;


namespace ParksComputing.Xferc;

internal class XfercProgram {
    private static readonly string _configFilePath;
    private static readonly string _xfercDirectory;

    static XfercProgram() {
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _xfercDirectory = Path.Combine(homeDirectory, Constants.XfercDirectoryName);

        if (!Directory.Exists(_xfercDirectory)) {
            Directory.CreateDirectory(_xfercDirectory);
        }

        _configFilePath = Path.Combine(_xfercDirectory, Constants.ConfigFileName);
    }

    static async Task<int> Main(string[] args) {
        var cli = new ClifferBuilder()
            .ConfigureAppConfiguration((configurationBuiler) => {
                configurationBuiler.AddJsonFile(_configFilePath, true);
            })
            .ConfigureServices(services => {
                services.AddSingleton<PersistenceService>();
            })
            .Build();

        Utility.SetServiceProvider(cli.ServiceProvider);

        ClifferEventHandler.OnExit += () => {
            var persistenceService = Utility.GetService<PersistenceService>()!;
        };

        return await cli.RunAsync(args);
    }
}
