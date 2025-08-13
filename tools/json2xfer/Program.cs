using System.Text;
using Cliffer;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Json2Xfer;

internal class Program {
    private static readonly string _configFilePath;
    private static readonly string _xfercDirectory;

    static Program() {
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _xfercDirectory = Path.Combine(homeDirectory, Constants.XfercDirectoryName);

        if (!Directory.Exists(_xfercDirectory)) {
            Directory.CreateDirectory(_xfercDirectory);
        }

        _configFilePath = Path.Combine(_xfercDirectory, Constants.ConfigFileName);
    }

    static async Task<int> Main(string[] args) {
        Console.OutputEncoding = Encoding.UTF8;

        var cli = new ClifferBuilder()
            .ConfigureAppConfiguration((configurationBuilder) => {
                configurationBuilder.AddJsonFile(_configFilePath, true);
            })
            .Build();

        Utility.SetServiceProvider(cli.ServiceProvider);

        ClifferEventHandler.OnExit += () => {
        };


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

