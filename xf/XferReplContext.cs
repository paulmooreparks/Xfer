using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Reflection;

using Cliffer;

using ParksComputing.Xfer.Cli.Services;
using ParksComputing.Xfer.Workspace.Services;

namespace ParksComputing.Xfer.Cli;

internal class XferReplContext : Cliffer.DefaultReplContext {
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkspaceService _workspaceService;
    private readonly CommandSplitter _commandSplitter;
    private readonly System.CommandLine.Option _recursionOption;

    public string Title => "Xfer CLI Application";
    public override string[] GetPopCommands() => [];

    public XferReplContext(
        IServiceProvider serviceProvider,
        IWorkspaceService workspaceService,
        CommandSplitter commandSplitter,
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
        return $"{command.Name}:{_workspaceService.CurrentWorkspaceName}> ";
    }


    public override string[] PreprocessArgs(string[] args, Command command, InvocationContext context) {
        var parseResult = command.Parse(args);

        if (parseResult != null && parseResult.Errors.Count > 0) {
            args[0] = $"{_workspaceService.CurrentWorkspaceName}.{args[0]}";
            parseResult = command.Parse(args);
        }

        var newArgs = new List<string>();
        newArgs.AddRange(args);

        if (parseResult?.Errors.Count == 0 && parseResult?.CommandResult.Command == command) {
            newArgs.Add(_recursionOption.Aliases.First());
            newArgs.Add("true");
        }

        return base.PreprocessArgs(newArgs.ToArray(), command, context);
    }

    public override Task<int> RunAsync(Command command, string[] args) {
        if (args.Length > 0 && args[0].Trim().ToLower() == "foo") {
            Console.WriteLine("Do foo stuff");
            return Task.FromResult(Result.Success);
        }

        return base.RunAsync(command, args);
    }

    public override string[] SplitCommandLine(string input) {
        return _commandSplitter.Split(input).ToArray();
    }
}
