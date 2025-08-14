using System.Text;
using Cliffer;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            .ConfigureAppConfiguration((configurationBuilder) => {
                configurationBuilder.AddJsonFile(_configFilePath, true);
            })
            .ConfigureServices(services => {
                services.AddSingleton<PersistenceService>();
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

internal abstract class Element<T> {
    public abstract T TypedValue { get; set; }
    public abstract string Value { get; }
}

internal class IntegerElement : Element<int> {
    public override int TypedValue { get; set; }
    public override string Value => TypedValue.ToString();
}
