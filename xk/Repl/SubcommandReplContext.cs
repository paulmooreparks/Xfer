using System.CommandLine;
using System.CommandLine.Invocation;

using Cliffer;
using ParksComputing.XferKit.Cli.Services;

namespace ParksComputing.XferKit.Cli;

internal class SubcommandReplContext : Cliffer.DefaultReplContext {
    private readonly ICommandSplitter _commandSplitter;

    public override string[] GetExitCommands() => ["exit"];
    public override string[] GetPopCommands() => ["/"];

    public SubcommandReplContext(
        ICommandSplitter commandSplitter
        ) 
    {
        _commandSplitter = commandSplitter;
    }

    override public string GetPrompt(Command command, InvocationContext context) {
        return $"{command.Name}> ";
    }

    public override async Task<int> RunAsync(Command command, string[] args) {
        // Prevent recursive invocation of the same command
        if (string.Equals(args[0], command.Name, StringComparison.OrdinalIgnoreCase)) {
            Console.WriteLine($"Already in '{command.Name}' context.");
            return Result.Success;
        }

        return await base.RunAsync(command, args);
    }

    public override string[] SplitCommandLine(string input) {
        return _commandSplitter.Split(input).ToArray();
    }
}
