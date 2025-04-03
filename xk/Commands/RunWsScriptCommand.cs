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

namespace ParksComputing.XferKit.Cli.Commands;

[Command("runwsscript", "Internal command to run workspace scripts.", IsHidden = true)]
[Option(typeof(string), "--scriptName", "The name of the script to run.", Arity = ArgumentArity.ExactlyOne)]
[Option(typeof(string), "--workspaceName", "The name of the workspace in which the script is contained.", Arity = ArgumentArity.ZeroOrOne)]
[Argument(typeof(IEnumerable<string>), "args", "Optional arguments", Arity = ArgumentArity.ZeroOrMore)]
internal class RunWsScriptCommand {
    private readonly IWorkspaceService _workspaceService;
    private readonly IScriptEngine _scriptEngine;

    public RunWsScriptCommand(
        IWorkspaceService workspaceService,
        IScriptEngine scriptEngine
        ) { 
        _workspaceService = workspaceService;
        _scriptEngine = scriptEngine;
    }

    public int Execute(
        string scriptName,
        string? workspaceName,
        IEnumerable<string>? args
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

        if (args is not null) {
            var quotedArgs = new List<string>();
            int i = 0;

            foreach (var arg in args) {
                if (i >= argumentDefinitions.Count()) {
                    Console.Error.WriteLine($"{Constants.ErrorChar} Too many arguments provided for script '{scriptName}'.");
                    return Result.Error;
                }

                var argType = argumentDefinitions[i].Key;

                switch (argType) {
                    case "string":
                        quotedArgs.Add((arg.StartsWith("\"") && arg.EndsWith("\"")) || (arg.StartsWith("'") && arg.EndsWith("'"))
                            ? arg
                            : $"\"{arg}\"");
                        break;

                    case "stringArray":
                        var stringArray = arg.Split(',').Select(a =>
                            (a.StartsWith("\"") && a.EndsWith("\"")) || (a.StartsWith("'") && a.EndsWith("'"))
                            ? a
                            : $"\"{a}\"");
                        quotedArgs.Add($"[{string.Join(", ", stringArray)}]");
                        break;

                    case "number":
                    case "boolean":
                    case "object":
                        quotedArgs.Add(arg);
                        break;

                    default:
                        Console.Error.WriteLine($"{Constants.ErrorChar} Unsupported argument type '{argType}' in script '{scriptName}'.");
                        return Result.Error;
                }

                i++;
            }

            if (i < argumentDefinitions.Count && Console.IsInputRedirected) {
                var argString = Console.In.ReadToEnd().Trim();
                var argType = argumentDefinitions[i].Key;

                if (argType == "string") {
                    quotedArgs.Add((argString.StartsWith("\"") && argString.EndsWith("\"")) || (argString.StartsWith("'") && argString.EndsWith("'"))
                        ? argString
                        : $"\"{argString}\"");
                }
                else if (argType == "stringArray") {
                    var stringArray = argString.Split(',').Select(a =>
                        (a.StartsWith("\"") && a.EndsWith("\"")) || (a.StartsWith("'") && a.EndsWith("'"))
                        ? a
                        : $"\"{a}\"");
                    quotedArgs.Add($"[{string.Join(", ", stringArray)}]");
                }
                else {
                    quotedArgs.Add(argString);
                }
            }

            paramList = string.Join(", ", quotedArgs);
        }

        string scriptBody = string.Empty;

        if (string.IsNullOrEmpty(workspaceName)) {
            found = _workspaceService.BaseConfig.Scripts.TryGetValue(scriptName, out scriptDefinition);

            if (!found) {
                Console.Error.WriteLine($"{Constants.ErrorChar} Script '{scriptName}' not found.");
                return Result.Error;
            }

            scriptBody = $"__script__{scriptName} ({paramList})";
        }
        else {
            if (_workspaceService.BaseConfig.Workspaces.TryGetValue(workspaceName, out var workspace)) {
                found = workspace.Scripts.TryGetValue(scriptName, out scriptDefinition);

                if (!found) {
                    Console.Error.WriteLine($"{Constants.ErrorChar} Script '{workspaceName}.{scriptName}' not found.");
                    return Result.Error;
                }

                scriptBody = $"__script__{workspaceName}__{scriptName} ({paramList})";
            }
            else {
                Console.Error.WriteLine($"{Constants.ErrorChar} Workspace '{workspaceName}' not found.");
                return Result.Error;
            }
        }

        var output = _scriptEngine.EvaluateScript(scriptBody);
        
        if (output != null) {
            Console.WriteLine(output);
        }
        return Result.Success;
    }
}
