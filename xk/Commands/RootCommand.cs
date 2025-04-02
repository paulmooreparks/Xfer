using Cliffer;
using ParksComputing.XferKit.Cli.Services;
using ParksComputing.XferKit.Workspace;
using ParksComputing.XferKit.Workspace.Services;
using ParksComputing.XferKit.Scripting.Services;
using ParksComputing.XferKit.Api;

using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.VisualBasic;

namespace ParksComputing.XferKit.Cli.Commands;
[RootCommand("Xfer CLI Application")]
[Option(typeof(string), "--baseurl", "The base URL of the API to send HTTP requests to.", new[] { "-b" }, IsRequired = false)]
[Option(typeof(string), "--workspace", "Path to a workspace file to use, other than the default.", new[] { "-w" }, IsRequired = false)]
[Option(typeof(bool), "--recursive", "Indicates if this is a recursive call.", IsHidden = true, IsRequired = false)]
internal class RootCommand {
    private readonly Option _recursionOption;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkspaceService _workspaceService;
    private readonly IReplContext _replContext;
    private readonly System.CommandLine.RootCommand _rootCommand;
    private readonly IScriptEngine _scriptEngine;
    private readonly XferKitApi _xk;

    private string _currentWorkspaceName = string.Empty;

    public RootCommand(
        IServiceProvider serviceProvider,
        IWorkspaceService workspaceService,
        System.CommandLine.RootCommand rootCommand,
        ICommandSplitter splitter,
        IScriptEngine scriptEngine,
        XferKitApi xferKitApi,
        [OptionParam("--recursive")] Option recursionOption
        ) 
    { 
        _serviceProvider = serviceProvider;
        _workspaceService = workspaceService;
        _rootCommand = rootCommand;
        _scriptEngine = scriptEngine;
        _xk = xferKitApi;
        _recursionOption = recursionOption;
        _replContext = new XferReplContext(_serviceProvider, _workspaceService, splitter, _recursionOption);

        if (_workspaceService.BaseConfig is not null) {
            foreach (var macro in _workspaceService.BaseConfig.Macros) {
                var macroCommand = new Macro($"{macro.Key}", $"[macro] {macro.Value.Description}", macro.Value.Command);

                _rootCommand.AddCommand(macroCommand);
            }

            foreach (var script in _workspaceService.BaseConfig.Scripts) {
                var description = script.Value.Description ?? string.Empty;
                var scriptName = script.Key;
                var scriptBody = script.Value.Script;
                var arguments = script.Value.Arguments;
                var paramList = new List<string>();
                var macroCommand = new Macro($"{scriptName}", $"[script] {description}", $"runwsscript --scriptName {scriptName}");

                foreach (var argument in arguments) {
                    var argType = argument.Value.Key;
                    var argName = argument.Key;
                    var argDescription = argument.Value.Value;

                    switch (argType) {
                        case "string":
                            macroCommand.AddArgument(new Argument<string>(argName, argDescription));
                            break;
                        case "number":
                            macroCommand.AddArgument(new Argument<double>(argName, argDescription));
                            break;
                        case "boolean":
                            macroCommand.AddArgument(new Argument<bool>(argName, argDescription));
                            break;
                        case "object":
                            macroCommand.AddArgument(new Argument<object>(argName, argDescription));
                            break;
                        default:
                            Console.Error.WriteLine($"{ParksComputing.XferKit.Workspace.Constants.ErrorChar} Script {scriptName}: Invalid or unsupported argument type {argType} for argument {argName}");
                            break;
                    }

                    paramList.Add(argument.Key);
                }

                _rootCommand.AddCommand(macroCommand);
                var paramString = string.Join(", ", paramList);

                _scriptEngine.ExecuteScript($@"
function __script__{scriptName}({paramString}) {{
{scriptBody}
}};

xk.{scriptName} = __script__{scriptName};
");
            }
        }

        foreach (var workspaceKvp in _workspaceService.BaseConfig?.Workspaces ?? new Dictionary<string, Workspace.Models.WorkspaceConfig>()) {
            var workspaceName = workspaceKvp.Key;
            var workspaceConfig = workspaceKvp.Value;

            foreach (var script in workspaceConfig.Scripts) {
                var description = script.Value.Description ?? string.Empty;
                var scriptName = script.Key;
                var scriptBody = script.Value.Script;
                var arguments = script.Value.Arguments;
                var paramList = new List<string>();
                var macroCommand = new Macro($"{workspaceName}.{scriptName}", $"[script] {description}", $"runwsscript --workspaceName {workspaceName} --scriptName {scriptName}");

                foreach (var argument in arguments) {
                    var argType = argument.Value.Key;
                    var argName = argument.Key;
                    var argDescription = argument.Value.Value;

                    switch (argType) {
                        case "string":
                            macroCommand.AddArgument(new Argument<string>(argName, argDescription));
                            break;
                        case "number":
                            macroCommand.AddArgument(new Argument<double>(argName, argDescription));
                            break;
                        case "boolean":
                            macroCommand.AddArgument(new Argument<bool>(argName, argDescription));
                            break;
                        case "object":
                            macroCommand.AddArgument(new Argument<object>(argName, argDescription));
                            break;
                        default:
                            Console.Error.WriteLine($"{ParksComputing.XferKit.Workspace.Constants.ErrorChar} Script {scriptName}: Invalid or unsupported argument type {argType} for argument {argName}");
                            break;
                    }

                    paramList.Add(argument.Key);
                }

                _rootCommand.AddCommand(macroCommand);
                var paramString = string.Join(", ", paramList);

                _scriptEngine.ExecuteScript($@"
function __script__{workspaceName}__{scriptName}({paramString}) {{
{scriptBody}
}};

xk.workspaces.{workspaceName}.{scriptName} = __script__{workspaceName}__{scriptName};

");
            }

            foreach (var request in workspaceConfig.Requests) {
                var description = request.Value.Description ?? $"{request.Value.Method} {request.Value.Endpoint}";
                var macroCommand = new Macro($"{workspaceName}.{request.Key}", $"[request] {description}", $"send {workspaceName}.{request.Key} --baseurl {workspaceKvp.Value.BaseUrl}");

                var baseurlOption = new Option<string>(["--baseurl", "-b"], "The base URL of the API to send HTTP requests to.");
                baseurlOption.IsRequired = false;
                macroCommand.AddOption(baseurlOption);

                var parameterOption = new Option<IEnumerable<string>>(["--parameters", "-p"], "Query parameters to include in the request. If input is redirected, parameters can also be read from standard input.");
                parameterOption.AllowMultipleArgumentsPerToken = true;
                parameterOption.Arity = System.CommandLine.ArgumentArity.ZeroOrMore;
                macroCommand.AddOption(parameterOption);

                var headersOption = new Option<IEnumerable<string>>(["--headers", "-h"], "Headers to include in the request.");
                headersOption.AllowMultipleArgumentsPerToken = true;
                headersOption.Arity = System.CommandLine.ArgumentArity.ZeroOrMore;
                macroCommand.AddOption(headersOption);

                var payloadOption = new Option<string>(["--payload", "-pl"], "Content to send with the request. If input is redirected, content can also be read from standard input.");
                payloadOption.Arity = System.CommandLine.ArgumentArity.ZeroOrOne;
                macroCommand.AddOption(payloadOption);

                _rootCommand.AddCommand(macroCommand);
            }

            foreach (var macro in workspaceConfig.Macros) {
                var macroCommand = new Macro($"{workspaceName}.{macro.Key}", $"[macro] {macro.Value.Description}", macro.Value.Command);

                _rootCommand.AddCommand(macroCommand);
            }
        }
    }

    public async Task<int> Execute(
        [OptionParam("--baseurl")] string? baseUrl,
        [OptionParam("--workspace")] string? workspace,
        [OptionParam("--recursive")] bool isRecursive,
        Command command,
        InvocationContext context
        ) 
    {
        if ( workspace is not null) {
        }

        if (baseUrl is not null) {
            _workspaceService.ActiveWorkspace.BaseUrl = baseUrl;
        }

        if (isRecursive) {
            return Result.Success;
        }

        var result = await command.Repl(
            _serviceProvider, 
            context,
            _replContext
            );

        return result;
    }
}
