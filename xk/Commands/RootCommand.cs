using Cliffer;
using ParksComputing.XferKit.Cli.Services;
using ParksComputing.XferKit.Workspace;
using ParksComputing.XferKit.Workspace.Services;
using ParksComputing.XferKit.Scripting.Services;
using ParksComputing.XferKit.Api;
using Microsoft.ClearScript;

using System.CommandLine;
using System.CommandLine.Invocation;

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
    private readonly IXferScriptEngine _scriptEngine;
    private readonly XferKitApi _xk;
    private readonly IScriptCliBridge _scriptCliBridge;

    private string _currentWorkspaceName = string.Empty;

    public RootCommand(
        IServiceProvider serviceProvider,
        IWorkspaceService workspaceService,
        System.CommandLine.RootCommand rootCommand,
        ICommandSplitter splitter,
        IXferScriptEngine scriptEngine,
        XferKitApi xferKitApi,
        IScriptCliBridge scriptCliBridge,
        [OptionParam("--recursive")] Option recursionOption
        ) {
        _serviceProvider = serviceProvider;
        _workspaceService = workspaceService;
        _rootCommand = rootCommand;
        _scriptEngine = scriptEngine;
        _xk = xferKitApi;
        _recursionOption = recursionOption;
        _replContext = new XferReplContext(_serviceProvider, _workspaceService, splitter, _recursionOption);
        _scriptCliBridge = scriptCliBridge;
        _scriptCliBridge.RootCommand = rootCommand;

#if false
        string functionScript = $@"
function myFunction(baseUrl, page) {{
    log(baseUrl);
    return page;
}}
";

        // Compile the function
        _scriptEngine.EvaluateScript(functionScript);

        string baseUrl = "https://example.com";
        string page = "https://example.com/page";

        // Call it with your values
        _scriptEngine.Script["myFunction"](baseUrl, page);
#endif
    }

    public void ConfigureWorkspaces(IClifferCli cli) {
        if (_workspaceService.BaseConfig is not null) {
            foreach (var macro in _workspaceService.BaseConfig.Macros) {
                var macroCommand = new Macro($"{macro.Key}", $"[macro] {macro.Value.Description}", macro.Value.Command);

                _rootCommand.AddCommand(macroCommand);
            }

            foreach (var script in _workspaceService.BaseConfig.Scripts) {
                var scriptName = script.Key;
                var scriptBody = script.Value.Script;
                var scriptCall = $"runwsscript --scriptName {scriptName}";
                var description = script.Value.Description ?? scriptCall;
                var arguments = script.Value.Arguments;
                var paramList = new List<string>();
                var macroCommand = new Macro($"{scriptName}", $"[script] {description}", scriptCall);

                foreach (var kvp in arguments) {
                    var argument = kvp.Value;
                    var argType = argument.Type;
                    var argName = kvp.Key;
                    argument.Name = argName;
                    var argDescription = argument.Description;
                    System.CommandLine.ArgumentArity argArity = argument.IsRequired ? System.CommandLine.ArgumentArity.ExactlyOne : System.CommandLine.ArgumentArity.ZeroOrOne;

                    switch (argType) {
                        case "string":
                            macroCommand.AddArgument(new Argument<string>(argName, argDescription) { Arity = argArity });
                            break;
                        case "number":
                            macroCommand.AddArgument(new Argument<double>(argName, argDescription) { Arity = argArity });
                            break;
                        case "boolean":
                            macroCommand.AddArgument(new Argument<bool>(argName, argDescription) { Arity = argArity });
                            break;
                        case "object":
                            macroCommand.AddArgument(new Argument<object>(argName, argDescription) { Arity = argArity });
                            break;
                        default:
                            Console.Error.WriteLine($"{ParksComputing.XferKit.Workspace.Constants.ErrorChar} Script {scriptName}: Invalid or unsupported argument type {argType} for argument {argName}");
                            break;
                    }

                    paramList.Add(argument.Name);
                }

                macroCommand.AddArgument(new Argument<IEnumerable<string>>("params", "Additional arguments.") { Arity = System.CommandLine.ArgumentArity.ZeroOrMore, IsHidden = true });
                paramList.Add("params");

                _rootCommand.AddCommand(macroCommand);
                var paramString = string.Join(", ", paramList);

                _scriptEngine.EvaluateScript($@"
function __script__{scriptName}({paramString}) {{
{scriptBody}
}};

xk.{scriptName} = __script__{scriptName};
");
            }
        }

        var workspaceColl = _xk.workspaces as IDictionary<string, object>;

        foreach (var workspaceKvp in _workspaceService.BaseConfig?.Workspaces ?? new Dictionary<string, Workspace.Models.WorkspaceConfig>()) {
            var workspaceName = workspaceKvp.Key;
            var workspaceConfig = workspaceKvp.Value;
            var workspace = workspaceColl![workspaceName] as dynamic;

            foreach (var script in workspaceConfig.Scripts) {
                var scriptName = script.Key;
                var scriptBody = script.Value.Script;
                var scriptCall = $"runwsscript --workspaceName {workspaceName} --scriptName {scriptName}";
                var description = script.Value.Description ?? scriptCall;
                var arguments = script.Value.Arguments;
                var paramList = new List<string>();
                var macroCommand = new Macro($"{workspaceName}.{scriptName}", $"[script] {description}", scriptCall);
                macroCommand.IsHidden = workspaceConfig.IsHidden;

                foreach (var kvp in arguments) {
                    var argument = kvp.Value;
                    var argType = argument.Type;
                    var argName = kvp.Key;
                    argument.Name = argName;
                    var argDescription = argument.Description;
                    System.CommandLine.ArgumentArity argArity = argument.IsRequired ? System.CommandLine.ArgumentArity.ExactlyOne : System.CommandLine.ArgumentArity.ZeroOrOne;

                    switch (argType) {
                        case "string":
                            macroCommand.AddArgument(new Argument<string>(argName, argDescription) { Arity = argArity });
                            break;
                        case "number":
                            macroCommand.AddArgument(new Argument<double>(argName, argDescription) { Arity = argArity });
                            break;
                        case "boolean":
                            macroCommand.AddArgument(new Argument<bool>(argName, argDescription) { Arity = argArity });
                            break;
                        case "object":
                            break;
                        default:
                            Console.Error.WriteLine($"{ParksComputing.XferKit.Workspace.Constants.ErrorChar} Script {scriptName}: Invalid or unsupported argument type {argType} for argument {argName}");
                            break;
                    }

                    paramList.Add(argument.Name);
                }

                macroCommand.AddArgument(new Argument<IEnumerable<string>>("params", "Additional arguments.") { Arity = System.CommandLine.ArgumentArity.ZeroOrOne, IsHidden = true });
                paramList.Add("params");

                _rootCommand.AddCommand(macroCommand);
                var paramString = string.Join(", ", paramList);

                _scriptEngine.EvaluateScript($@"
function __script__{workspaceName}__{scriptName}(workspace, {paramString}) {{
{scriptBody}
}};

xk.workspaces.{workspaceName}.{scriptName} = __script__{workspaceName}__{scriptName};
");
            }

            var requests = workspace.requests as IDictionary<string, object>;

            foreach (var requestKvp in workspaceConfig.Requests) {
                var request = requestKvp.Value;
                var requestName = requestKvp.Key;
                request.Name = requestName;
                var description = request.Description ?? $"{request.Method} {request.Endpoint}";
                var scriptCall = $"send {workspaceName} {requestName} --baseurl {workspaceKvp.Value.BaseUrl}";
                var macroCommand = new Macro($"{workspaceName}.{requestName}", $"[request] {description}", scriptCall);
                macroCommand.IsHidden = workspaceConfig.IsHidden;

                var requestObj = requests![requestName] as IDictionary<string, object>;
                var requestCaller = new RequestCaller(cli, workspaceName, requestName, workspaceKvp.Value.BaseUrl);
#pragma warning disable CS8974 // Converting method group to non-delegate type
                requestObj!["execute"] = requestCaller.RunRequest;
                // requestObj!["execute"] = new Func<object?[], Task<object?>>(requestCaller.RunRequest);
                _scriptEngine.AddHostObject($"reqTest_{workspaceName}_{requestName}", requestCaller.RunRequest);
                // _scriptEngine.AddHostObject($"reqTest_{workspaceName}_{requestName}", new Func<object?[], Task<object?>>(requestCaller.RunRequest));

#pragma warning restore CS8974 // Converting method group to non-delegate type

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

                foreach (var kvp in request.Arguments) {
                    var argName = kvp.Key;
                    var argument = kvp.Value;
                    argument.Name = argName;
                    var argType = argument.Type;
                    var argDescription = argument.Description;
                    System.CommandLine.ArgumentArity argArity = argument.IsRequired ? System.CommandLine.ArgumentArity.ExactlyOne : System.CommandLine.ArgumentArity.ZeroOrOne;

                    switch (argType) {
                        case "string":
                            macroCommand.AddArgument(new Argument<string>(argName, argDescription) { Arity = argArity });
                            break;
                        case "number":
                            macroCommand.AddArgument(new Argument<double>(argName, argDescription) { Arity = argArity });
                            break;
                        case "boolean":
                            macroCommand.AddArgument(new Argument<bool>(argName, argDescription) { Arity = argArity });
                            break;
                        case "object":
                            macroCommand.AddArgument(new Argument<object>(argName, argDescription) { Arity = argArity });
                            break;
                        default:
                            Console.Error.WriteLine($"{ParksComputing.XferKit.Workspace.Constants.ErrorChar} Request {requestName}: Invalid or unsupported argument type {argType} for argument {argName}");
                            break;
                    }
                }

                _rootCommand.AddCommand(macroCommand);
            }

            foreach (var macro in workspaceConfig.Macros) {
                var macroCommand = new Macro($"{workspaceName}.{macro.Key}", $"[macro] {macro.Value.Description}", macro.Value.Command);
                macroCommand.IsHidden = workspaceConfig.IsHidden;

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

public class RequestCaller {
    private readonly IClifferCli? _cli;

    public string WorkspaceName { get; set; }
    public string RequestName { get; set; }
    public string BaseUrl { get; set; }

    public RequestCaller(
        IClifferCli? cli,
        string workspaceName,
        string requestName,
        string? baseUrl
        ) 
    {
        _cli = cli;
        WorkspaceName = workspaceName;
        RequestName = requestName;
        BaseUrl = baseUrl!;
    }

    public object? RunRequest(params object?[]? args) {
        if (_cli is null) {
            return null;
        }

        if (_cli.Commands.TryGetValue("send", out object? commandObject)) {
            if (commandObject is SendCommand sendCommand) {
                var result = sendCommand.DoCommand(WorkspaceName, RequestName, BaseUrl, null, null, null, null, null, args, _cli);
                return sendCommand.CommandResult;
            }
        }

        return null;
    }
}
