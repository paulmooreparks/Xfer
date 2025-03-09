using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;
using System.CommandLine;
using System.CommandLine.Invocation;
using ParksComputing.XferKit.Cli.Services;
using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Cli;

internal class ScriptReplContext : Cliffer.DefaultReplContext {
    private readonly IScriptEngine _scriptEngine;
    private readonly ICommandSplitter _commandSplitter;
    private readonly IWorkspaceService _workspaceService;

    public ScriptReplContext(
        IScriptEngine scriptEngine,
        ICommandSplitter commandSplitter,
        IWorkspaceService workspaceService
        ) 
    {
        _scriptEngine = scriptEngine;
        _commandSplitter = commandSplitter;
        _workspaceService = workspaceService;
    }

    public override string[] GetExitCommands() => ["exit"];
    public override string[] GetPopCommands() => ["quit"];
    public override string[] GetHelpCommands() => ["-?", "-h", "--help"];

    override public string GetPrompt(Command command, InvocationContext context) {
        return $"{command.Name}> ";
    }

    public override void OnEntry() {
        base.OnEntry();
    }

    public override string[] SplitCommandLine(string input) {
        return _commandSplitter.Split(input).ToArray();
    }

    public override Task<int> RunAsync(Command command, string[] args) {
        var helpCommands = GetHelpCommands();
        var isHelp = helpCommands.Contains(args[0]);

        if (args.Length > 0 && !isHelp) {
            var script = string.Join(' ', args);
            var output = _scriptEngine.ExecuteCommand(script);
            Console.WriteLine(output);
            return Task.FromResult(Result.Success);
        }

        ClifferEventHandler.PreprocessArgs(args);
        return base.RunAsync(command, args);
    }
}
