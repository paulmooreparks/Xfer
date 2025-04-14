using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Cli.Services;
using ParksComputing.XferKit.Workspace;
using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Cli;

internal class WorkspaceReplContext : Cliffer.DefaultReplContext {
    private readonly ICommandSplitter _commandSplitter;
    private readonly IWorkspaceService _workspaceService;
    private readonly RootCommand _rootCommand;

    public WorkspaceReplContext(
        RootCommand rootCommand,
        ICommandSplitter commandSplitter,
        IWorkspaceService workspaceService
        ) 
    {
        _rootCommand = rootCommand;
        _commandSplitter = commandSplitter;
        _workspaceService = workspaceService;
    }

    public override string[] GetExitCommands() => ["exit"];
    public override string[] GetPopCommands() => ["/"];
    // public override string[] GetHelpCommands() => ["-?", "-h", "--help"];

    override public string GetPrompt(Command command, InvocationContext context) {
        return $"{_workspaceService.CurrentWorkspaceName}> ";
    }

    public override string[] PreprocessArgs(string[] args, Command command, InvocationContext context) {
        return base.PreprocessArgs(args, command, context);
    }

    public override async Task<int> RunAsync(Command workspaceCommand, string[] args) {
        if (args.Length == 0) {
            return Result.Success;
        }

        // First: Try parsing in the workspace
        var workspaceParser = new Parser(workspaceCommand);
        var parseResult = workspaceParser.Parse(args);

        if (parseResult.Errors.Count > 0) {
            // Try fallback to root parser
            var rootParser = new Parser(_rootCommand);
            var rootParseResult = rootParser.Parse(args);

            if (rootParseResult.Errors.Count == 0 && rootParseResult.CommandResult.Command != _rootCommand) {
                return await _rootCommand.InvokeAsync(args);
            }

            // No match in either workspace or root
            Console.Error.WriteLine($"{Constants.ErrorChar} {parseResult.Errors[0].Message}");
            return Result.ErrorInvalidArgument;
        }

        if (parseResult.Directives.Count() > 0) {
            Console.Error.WriteLine($"{Constants.ErrorChar} Unknown directive: {parseResult.Directives.First()}");
            return Result.ErrorInvalidArgument;
        }

        if (parseResult.CommandResult.Command == workspaceCommand) {
            // User typed a help command
            return await workspaceCommand.InvokeAsync(args);
        }

        if (parseResult.CommandResult.Command is Command matchedSubCommand) {
            return await matchedSubCommand.InvokeAsync(args);
        }

        return await base.RunAsync(workspaceCommand, args);
    }

    public override string[] SplitCommandLine(string input) {
        return _commandSplitter.Split(input).ToArray();
    }
}
