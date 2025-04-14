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
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace ParksComputing.XferKit.Cli.Commands;

internal class RunWsScriptCommand {
    private readonly IWorkspaceService _workspaceService;
    private readonly IXferScriptEngine _scriptEngine;

    public object? CommandResult { get; private set; } = null;

    public RunWsScriptCommand(
        IWorkspaceService workspaceService,
        IXferScriptEngine scriptEngine
        ) 
    { 
        _workspaceService = workspaceService;
        _scriptEngine = scriptEngine;
    }

    public int Handler(
        InvocationContext invocationContext,
        string scriptName,
        string? workspaceName
        ) 
    {
        var parseResult = invocationContext.ParseResult;
        return Execute(invocationContext, scriptName, workspaceName, [], parseResult.CommandResult.Tokens);
    }

    public int Execute(
        InvocationContext invocationContext,
        string scriptName,
        string? workspaceName,
        IEnumerable<object>? args,
        IReadOnlyList<System.CommandLine.Parsing.Token>? tokenArguments
        ) 
    {
        var result = DoCommand(invocationContext, scriptName, workspaceName, args, tokenArguments);

        if (CommandResult is not null && !CommandResult.Equals(Undefined.Value)) {
            Console.WriteLine(CommandResult);
        }

        return result;
    }

    public int DoCommand(
        InvocationContext invocationContext,
        string scriptName,
        string? workspaceName,
        IEnumerable<object>? args,
        IReadOnlyList<System.CommandLine.Parsing.Token>? tokenArguments
        ) 
    {
        if (scriptName is null) {
            Console.Error.WriteLine($"{Constants.ErrorChar} Script name is required.");
            return Result.Error;
        }

        var paramList = string.Empty;
        var scriptParams = new List<object?>();
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
        
        if (!string.IsNullOrEmpty(workspaceName)) {
            var workspaces = _scriptEngine.Script.xk.workspaces as IDictionary<string, object?>;
            if (workspaces is not null) {
                var workspaceObj = workspaces[workspaceName];
                scriptParams.Add(workspaceObj);
            }
        }

        if (tokenArguments is not null && tokenArguments.Any()) {
            int i = 0;
            using var enumerator = tokenArguments.GetEnumerator();

            foreach (var token in tokenArguments) {
                var arg = token.Value;

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
        }
        else if (args is not null) {
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
