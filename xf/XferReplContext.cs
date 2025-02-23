using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Reflection;

using Cliffer;

using ParksComputing.Xfer.Cli.Services;

namespace ParksComputing.Xfer.Cli;

internal class XferReplContext : Cliffer.DefaultReplContext {
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkspaceService _workspaceService;

    public string Title => "Xfer CLI Application";
    public override string[] GetPopCommands() => [];

    public XferReplContext(
        IServiceProvider serviceProvider,
        IWorkspaceService workspaceService
        ) 
    {
        _serviceProvider = serviceProvider;
        _workspaceService = workspaceService;
    }

    public override string GetTitleMessage() {
        Assembly assembly = Assembly.GetExecutingAssembly();
        Version? version = assembly.GetName().Version;
        string versionString = version?.ToString() ?? "Unknown";
        return $"{Title} v{versionString}";
    }

    public override string[] PreprocessArgs(string[] args, Command command, InvocationContext context) {
        return base.PreprocessArgs(args, command, context);
    }

    public override Task<int> RunAsync(Command command, string[] args) {
        if (args.Length > 0 && args[0].Trim().ToLower() == "foo") {
            Console.WriteLine("Do foo stuff");
            return Task.FromResult(Result.Success);
        }

        return base.RunAsync(command, args);
    }
}
