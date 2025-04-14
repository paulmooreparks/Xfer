using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Reflection;

using Cliffer;
using ParksComputing.XferKit.Cli.Services;
using ParksComputing.XferKit.Workspace;
using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Cli;

internal class XkReplContext : Cliffer.DefaultReplContext {
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkspaceService _workspaceService;
    private readonly ICommandSplitter _commandSplitter;
    private readonly System.CommandLine.Option _recursionOption;

    public string Title => "Xfer CLI Application";
    public override string[] GetPopCommands() => [];

    public XkReplContext(
        IServiceProvider serviceProvider,
        IWorkspaceService workspaceService,
        ICommandSplitter commandSplitter,
        System.CommandLine.Option recursionOption
        ) 
    {
        _serviceProvider = serviceProvider;
        _workspaceService = workspaceService;
        _commandSplitter = commandSplitter;
        _recursionOption = recursionOption;
    }

    public override string GetTitleMessage() {
        Assembly assembly = Assembly.GetExecutingAssembly();
        Version? version = assembly.GetName().Version;
        string versionString = version?.ToString() ?? "Unknown";
        return $"{Title} v{versionString}";
    }

    override public string GetPrompt(Command command, InvocationContext context) {
        return $"{command.Name}> ";
    }


    public override string[] PreprocessArgs(string[] args, Command command, InvocationContext context) {
        var newArgs = new List<string>();
        newArgs.AddRange(args);

        if (args[0] == command.Name) {
            newArgs.Add(_recursionOption.Aliases.First());
            newArgs.Add("true");
        }

        return base.PreprocessArgs(newArgs.ToArray(), command, context);
    }

    public override async Task<int> RunAsync(Command command, string[] args) {
        return await base.RunAsync(command, args);
    }

    public override string[] SplitCommandLine(string input) {
        return _commandSplitter.Split(input).ToArray();
    }
}
