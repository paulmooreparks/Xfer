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
using ParksComputing.XferKit.Scripting.Services;

namespace ParksComputing.XferKit.Cli;

internal class ScriptReplContext : Cliffer.DefaultReplContext {
    private readonly IXferScriptEngine _scriptEngine;
    private readonly ICommandSplitter _commandSplitter;
    private readonly IWorkspaceService _workspaceService;

    public ScriptReplContext(
        IXferScriptEngine scriptEngine,
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

    public override async Task<int> RunAsync(Command command, string[] args) {
        var helpCommands = GetHelpCommands();
        var isHelp = helpCommands.Contains(args[0]);

        if (args.Length > 0 && !isHelp) {
            var script = string.Join(' ', args);
            var result = _scriptEngine.EvaluateScript(script);

            if (result is Task taskResult) {
                await taskResult.ConfigureAwait(false);

                // Check if it's a Task<T> with a result
                var taskType = taskResult.GetType();
                if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>)) {
                    var property = taskType.GetProperty("Result");
                    var taskResultValue = property?.GetValue(taskResult);
                    if (taskResultValue is not null) {
                        Console.WriteLine(taskResultValue);
                    }
                }
            }
            else if (result is ValueTask valueTaskResult) {
                await valueTaskResult.ConfigureAwait(false);
            }
            else {
                if (result is not null) {
                    Console.WriteLine(result.ToString());
                }
            }
            
            return Result.Success;
        }

        ClifferEventHandler.PreprocessArgs(args);
        return await base.RunAsync(command, args);
    }
}
