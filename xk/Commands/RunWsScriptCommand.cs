using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Workspace;
using ParksComputing.XferKit.Workspace.Models;
using ParksComputing.XferKit.Workspace.Services;
using ParksComputing.XferKit.Scripting.Services;
using Microsoft.ClearScript;

namespace ParksComputing.XferKit.Cli.Commands;

[Command("runwsscript", "Internal command to run workspace scripts.", IsHidden = true)]
[Option(typeof(string), "--scriptName", "The name of the script to run.", Arity = ArgumentArity.ExactlyOne)]
[Option(typeof(string), "--workspaceName", "The name of the workspace in which the script is contained.", Arity = ArgumentArity.ZeroOrOne)]
[Argument(typeof(IEnumerable<string>), "params", "Optional arguments", Arity = ArgumentArity.ZeroOrMore)]
internal class RunWsScriptCommand {
    private readonly IWorkspaceService _workspaceService;
    private readonly IXferScriptEngine _scriptEngine;

    public object? CommandResult { get; private set; } = null;

    public RunWsScriptCommand(
        IWorkspaceService workspaceService,
        IXferScriptEngine scriptEngine
        ) { 
        _workspaceService = workspaceService;
        _scriptEngine = scriptEngine;
    }

    public int Execute(
        string scriptName,
        string? workspaceName,
        [ArgumentParam("params")] IEnumerable<object>? args
        ) 
    {
        var result = DoCommand( scriptName, workspaceName, args );

        if (CommandResult is not null && !CommandResult.Equals(Undefined.Value)) {
            Console.WriteLine(CommandResult);
        }

        return result;
    }

    public int DoCommand(
        string scriptName,
        string? workspaceName,
        [ArgumentParam("params")] IEnumerable<object>? args
        ) 
    {
        if (scriptName is null) {
            Console.Error.WriteLine($"{Constants.ErrorChar} Script name is required.");
            return Result.Error;
        }

        ScriptDefinition? scriptDefinition = null;
        bool found = false;

        if (string.IsNullOrEmpty(workspaceName)) {
            found = _workspaceService.BaseConfig.Scripts.TryGetValue(scriptName, out scriptDefinition);
            if (!found) {
                Console.Error.WriteLine($"{Constants.ErrorChar} Script '{scriptName}' not found.");
                return Result.Error;
            }
        }
        else {
            if (_workspaceService.BaseConfig.Workspaces.TryGetValue(workspaceName, out var workspace)) {
                found = workspace.Scripts.TryGetValue(scriptName, out scriptDefinition);
                if (!found) {
                    Console.Error.WriteLine($"{Constants.ErrorChar} Script '{workspaceName}.{scriptName}' not found.");
                    return Result.Error;
                }
            }
            else {
                Console.Error.WriteLine($"{Constants.ErrorChar} Workspace '{workspaceName}' not found.");
                return Result.Error;
            }
        }

        if (scriptDefinition is null) {
            Console.Error.WriteLine($"{Constants.ErrorChar} Script definition not found");
            return Result.Error;
        }

        var argumentDefinitions = scriptDefinition.Arguments.Values.ToList();
        var paramList = string.Empty;
        var scriptParams = new List<object?>();
        
        if (!string.IsNullOrEmpty(workspaceName)) {
            var workspaces = _scriptEngine.Script.xk.workspaces as IDictionary<string, object?>;
            if (workspaces is not null) {
                var workspaceObj = workspaces[workspaceName];
                scriptParams.Add(workspaceObj);
            }
        }

        if (args is not null) {
            int i = 0;

            foreach (var arg in args) {
                if (i >= argumentDefinitions.Count()) {
                    scriptParams.Add(arg);
                }
                else {
                    var argType = argumentDefinitions[i].Type;

                    switch (argType) {
                        case "string":
                            scriptParams.Add(arg);
                            break;

                        case "stringArray":
                            scriptParams.Add(arg);
                            break;

                        case "number":
                            scriptParams.Add(Convert.ToDouble(arg));
                            break;

                        case "boolean":
                            scriptParams.Add(Convert.ToBoolean(arg));
                            break;

                        case "object":
                            scriptParams.Add(arg);
                            break;

                        default:
                            Console.Error.WriteLine($"{Constants.ErrorChar} Unsupported argument type '{argType}' in script '{scriptName}'.");
                            return Result.Error;
                    }
                }

                i++;
            }

            if (i < argumentDefinitions.Count && Console.IsInputRedirected) {
                var argString = Console.In.ReadToEnd().Trim();
                var argType = argumentDefinitions[i].Name;

                if (argType == "string") {
                    scriptParams.Add(argString);
                }
                else if (argType == "stringArray") {
                    scriptParams.Add(argString);
                }
                else if (argType == "number") {
                    scriptParams.Add(Convert.ToDouble(argString));
                }
                else if (argType == "boolean") {
                    scriptParams.Add(Convert.ToBoolean(argString));
                }
                else if (argType == "object") {
                    scriptParams.Add(argString);
                }
                else {
                    scriptParams.Add(argString);
                }
            }
        }

        string scriptBody = string.Empty;

        if (string.IsNullOrEmpty(workspaceName)) {
            found = _workspaceService.BaseConfig.Scripts.TryGetValue(scriptName, out scriptDefinition);

            if (!found) {
                Console.Error.WriteLine($"{Constants.ErrorChar} Script '{scriptName}' not found.");
                return Result.Error;
            }

            scriptBody = $"__script__{scriptName}";
        }
        else {
            if (_workspaceService.BaseConfig.Workspaces.TryGetValue(workspaceName, out var workspace)) {
                found = workspace.Scripts.TryGetValue(scriptName, out scriptDefinition);

                if (!found) {
                    Console.Error.WriteLine($"{Constants.ErrorChar} Script '{workspaceName}.{scriptName}' not found.");
                    return Result.Error;
                }

                scriptBody = $"__script__{workspaceName}__{scriptName}";
            }
            else {
                Console.Error.WriteLine($"{Constants.ErrorChar} Workspace '{workspaceName}' not found.");
                return Result.Error;
            }
        }

        CommandResult = _scriptEngine.Invoke(scriptBody, scriptParams.ToArray());
        return Result.Success;
    }
}
