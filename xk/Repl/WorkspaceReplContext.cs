using System.CommandLine;
using System.CommandLine.Invocation;

using Cliffer;
using ParksComputing.XferKit.Cli.Services;
using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Cli.Repl;

internal class WorkspaceReplContext : DefaultReplContext
{
    private readonly ICommandSplitter _commandSplitter;
    private readonly IWorkspaceService _workspaceService;

    public override string[] GetExitCommands() => ["exit"];
    public override string[] GetPopCommands() => ["/"];

    public WorkspaceReplContext(
        ICommandSplitter commandSplitter,
        IWorkspaceService workspaceService
        )
    {
        _commandSplitter = commandSplitter;
        _workspaceService = workspaceService;
    }

    override public string GetPrompt(Command command, InvocationContext context) {
        return $"{_workspaceService.CurrentWorkspaceName}> ";
    }

    public override async Task<int> RunAsync(Command command, string[] args) {
        // Prevent recursive invocation of the same command
        if (string.Equals(args[0], command.Name, StringComparison.OrdinalIgnoreCase)) {
            Console.Error.WriteLine($"Already in '{command.Name}' context.");
            return Result.Success;
        }

        return await base.RunAsync(command, args);
    }

    public override string[] SplitCommandLine(string input) {
        return _commandSplitter.Split(input).ToArray();
    }
}
