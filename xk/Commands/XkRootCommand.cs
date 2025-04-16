﻿using Cliffer;
using ParksComputing.XferKit.Cli.Services;
using ParksComputing.XferKit.Workspace.Services;
using ParksComputing.XferKit.Scripting.Services;
using ParksComputing.XferKit.Api;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;

using System.Diagnostics;
using ParksComputing.XferKit.Cli.Services.Impl;
using ParksComputing.XferKit.Cli.Repl;
using ParksComputing.XferKit.Http.Services;


namespace ParksComputing.XferKit.Cli.Commands;
[RootCommand("Xfer CLI Application")]
[Option(typeof(string), "--baseurl", "The base URL of the API to send HTTP requests to.", new[] { "-b" }, IsRequired = false)]
[Option(typeof(string), "--workspace", "Path to a workspace file to use, other than the default.", new[] { "-w" }, IsRequired = false)]
[Option(typeof(bool), "--recursive", "Indicates if this is a recursive call.", IsHidden = true, IsRequired = false)]
internal class XkRootCommand {
    private readonly Option _recursionOption;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkspaceService _workspaceService;
    private readonly IReplContext _replContext;
    private readonly System.CommandLine.RootCommand _rootCommand;
    private readonly IXferScriptEngine _scriptEngine;
    private readonly XferKitApi _xk;
    private readonly IScriptCliBridge _scriptCliBridge;

    private string _currentWorkspaceName = string.Empty;

    public XkRootCommand(
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
        _replContext = new XkReplContext(_serviceProvider, _workspaceService, splitter, _recursionOption);
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

    public void ConfigureWorkspaces() {
        if (_workspaceService.BaseConfig is not null) {
            foreach (var macro in _workspaceService.BaseConfig.Macros) {
                var macroCommand = new Macro($"{macro.Key}", $"[macro] {macro.Value.Description}", macro.Value.Command);

                _rootCommand.AddCommand(macroCommand);
            }

            foreach (var script in _workspaceService.BaseConfig.Scripts) {
                var scriptName = script.Key;
                var scriptBody = script.Value.Script;
                var description = script.Value.Description ?? string.Empty;
                var arguments = script.Value.Arguments;
                var paramList = new List<string>();

                var scriptCommand = new Command(scriptName, $"[script] {description}");
                var scriptHandler = new RunWsScriptCommand(_workspaceService, _scriptEngine);

                foreach (var kvp in arguments) {
                    var argument = kvp.Value;
                    var argType = argument.Type;
                    var argName = kvp.Key;
                    argument.Name = argName;
                    var argDescription = argument.Description;
                    System.CommandLine.ArgumentArity argArity = argument.IsRequired ? System.CommandLine.ArgumentArity.ExactlyOne : System.CommandLine.ArgumentArity.ZeroOrOne;

                    switch (argType) {
                        case "string":
                            scriptCommand.AddArgument(new Argument<string>(argName, argDescription) { Arity = argArity });
                            break;
                        case "number":
                            scriptCommand.AddArgument(new Argument<double>(argName, argDescription) { Arity = argArity });
                            break;
                        case "boolean":
                            scriptCommand.AddArgument(new Argument<bool>(argName, argDescription) { Arity = argArity });
                            break;
                        case "object":
                            scriptCommand.AddArgument(new Argument<object>(argName, argDescription) { Arity = argArity });
                            break;
                        default:
                            Console.Error.WriteLine($"{ParksComputing.XferKit.Workspace.Constants.ErrorChar} Script {scriptName}: Invalid or unsupported argument type {argType} for argument {argName}");
                            break;
                    }

                    paramList.Add(argument.Name);
                }

                scriptCommand.AddArgument(new Argument<IEnumerable<string>>("params", "Additional arguments.") { Arity = System.CommandLine.ArgumentArity.ZeroOrMore, IsHidden = true });
                scriptCommand.Handler = CommandHandler.Create(int (InvocationContext invocationContext) => {
                    return scriptHandler.Handler(invocationContext, scriptName, null);
                });

                _rootCommand.AddCommand(scriptCommand);
                var paramString = string.Join(", ", paramList);

                try {
                    _scriptEngine.EvaluateScript($@"
function __script__{scriptName}({paramString}) {{
{scriptBody}
}};

xk.{scriptName} = __script__{scriptName};
");
                }
                catch (Exception ex) {
                    Console.Error.WriteLine($"{Workspace.Constants.ErrorChar} Error: {ex.Message}");
                }
            }
        }

        var workspaceColl = _xk.workspaces as IDictionary<string, object>;

        foreach (var workspaceKvp in _workspaceService.BaseConfig?.Workspaces ?? new Dictionary<string, Workspace.Models.WorkspaceConfig>()) {
            var workspaceName = workspaceKvp.Key;
            var workspaceConfig = workspaceKvp.Value;
            var workspace = workspaceColl![workspaceName] as dynamic;

            var workspaceCommand = new Command(workspaceName, $"[workspace] {workspaceConfig.Description}");
            workspaceCommand.IsHidden = workspaceConfig.IsHidden;
            var workspaceHandler = new WorkspaceCommand(workspaceName, _serviceProvider, _workspaceService);

            workspaceCommand.Handler = CommandHandler.Create(async Task<int> (InvocationContext invocationContext) => {
                return await workspaceHandler.Execute(workspaceCommand, invocationContext);
            });

            _rootCommand.AddCommand(workspaceCommand);

            foreach (var script in workspaceConfig.Scripts) {
                var scriptName = script.Key;
                var scriptBody = script.Value.Script;
                var description = script.Value.Description ?? string.Empty;
                var arguments = script.Value.Arguments;
                var paramList = new List<string>();

                var scriptCommand = new Command(scriptName, $"[script] {description}");
                scriptCommand.IsHidden = workspaceConfig.IsHidden;
                var scriptHandler = new RunWsScriptCommand(_workspaceService, _scriptEngine);

                foreach (var kvp in arguments) {
                    var argument = kvp.Value;
                    var argType = argument.Type;
                    var argName = kvp.Key;
                    argument.Name = argName;
                    var argDescription = argument.Description;
                    System.CommandLine.ArgumentArity argArity = argument.IsRequired ? System.CommandLine.ArgumentArity.ExactlyOne : System.CommandLine.ArgumentArity.ZeroOrOne;

                    switch (argType) {
                        case "string":
                            scriptCommand.AddArgument(new Argument<string>(argName, argDescription) { Arity = argArity });
                            break;
                        case "number":
                            scriptCommand.AddArgument(new Argument<double>(argName, argDescription) { Arity = argArity });
                            break;
                        case "boolean":
                            scriptCommand.AddArgument(new Argument<bool>(argName, argDescription) { Arity = argArity });
                            break;
                        case "object":
                            break;
                        default:
                            Console.Error.WriteLine($"{ParksComputing.XferKit.Workspace.Constants.ErrorChar} Script {scriptName}: Invalid or unsupported argument type {argType} for argument {argName}");
                            break;
                    }

                    paramList.Add(argument.Name);
                }

                scriptCommand.Handler = CommandHandler.Create(int (InvocationContext invocationContext) => {
                    return scriptHandler.Handler(invocationContext, scriptName, workspaceName);
                });

                workspaceCommand.AddCommand(scriptCommand);

                paramList.Add("params");

                var paramString = string.Join(", ", paramList);

                try {
                    _scriptEngine.EvaluateScript($@"
function __script__{workspaceName}__{scriptName}(workspace, {paramString}) {{
{scriptBody}
}};

xk.workspaces.{workspaceName}.{scriptName} = __script__{workspaceName}__{scriptName};
");
                }
                catch (Exception ex) {
                    Console.Error.WriteLine($"{Workspace.Constants.ErrorChar} Error: {ex.Message}");
                }
            }

            var requests = workspace.requests as IDictionary<string, object>;

            foreach (var requestKvp in workspaceConfig.Requests) {
                var request = requestKvp.Value;
                var requestName = requestKvp.Key;
                request.Name = requestName;
                var description = request.Description ?? $"{request.Method} {request.Endpoint}";
                var scriptCall = $"send {workspaceName} {requestName} --baseurl {workspaceKvp.Value.BaseUrl}";
                

                var requestCommand = new Command(requestName, $"[request] {description}");
                requestCommand.IsHidden = workspaceConfig.IsHidden;
                var requestHandler = new SendCommand(Utility.GetService<IHttpService>()!, _xk, _workspaceService, Utility.GetService<IXferScriptEngine>()!, Utility.GetService<IPropertyResolver>());
                var requestObj = requests![requestName] as IDictionary<string, object>;
                var requestCaller = new RequestCaller(_rootCommand, requestHandler, workspaceName, requestName, workspaceKvp.Value.BaseUrl);
#pragma warning disable CS8974 // Converting method group to non-delegate type
                requestObj!["execute"] = requestCaller.RunRequest;
#pragma warning restore CS8974 // Converting method group to non-delegate type

                var baseurlOption = new Option<string>(["--baseurl", "-b"], "The base URL of the API to send HTTP requests to.");
                baseurlOption.IsRequired = false;
                requestCommand.AddOption(baseurlOption);

                var parameterOption = new Option<IEnumerable<string>>(["--parameters", "-p"], "Query parameters to include in the request. If input is redirected, parameters can also be read from standard input.");
                parameterOption.AllowMultipleArgumentsPerToken = true;
                parameterOption.Arity = System.CommandLine.ArgumentArity.ZeroOrMore;
                requestCommand.AddOption(parameterOption);

                var headersOption = new Option<IEnumerable<string>>(["--headers", "-h"], "Headers to include in the request.");
                headersOption.AllowMultipleArgumentsPerToken = true;
                headersOption.Arity = System.CommandLine.ArgumentArity.ZeroOrMore;
                requestCommand.AddOption(headersOption);

                var payloadOption = new Option<string>(["--payload", "-pl"], "Content to send with the request. If input is redirected, content can also be read from standard input.");
                payloadOption.Arity = System.CommandLine.ArgumentArity.ZeroOrOne;
                requestCommand.AddOption(payloadOption);

                foreach (var kvp in request.Arguments) {
                    var argName = kvp.Key;
                    var argument = kvp.Value;
                    argument.Name = argName;
                    var argType = argument.Type;
                    var argDescription = argument.Description;
                    System.CommandLine.ArgumentArity argArity = argument.IsRequired ? System.CommandLine.ArgumentArity.ExactlyOne : System.CommandLine.ArgumentArity.ZeroOrOne;

                    switch (argType) {
                        case "string":
                            requestCommand.AddArgument(new Argument<string>(argName, argDescription) { Arity = argArity });
                            break;
                        case "number":
                            requestCommand.AddArgument(new Argument<double>(argName, argDescription) { Arity = argArity });
                            break;
                        case "boolean":
                            requestCommand.AddArgument(new Argument<bool>(argName, argDescription) { Arity = argArity });
                            break;
                        case "object":
                            requestCommand.AddArgument(new Argument<object>(argName, argDescription) { Arity = argArity });
                            break;
                        default:
                            Console.Error.WriteLine($"{ParksComputing.XferKit.Workspace.Constants.ErrorChar} Request {requestName}: Invalid or unsupported argument type {argType} for argument {argName}");
                            break;
                    }
                }

                requestCommand.Handler = CommandHandler.Create(int (InvocationContext invocationContext) => {
                    return requestHandler.Handler(invocationContext, workspaceName, requestName, workspaceConfig.BaseUrl, null, null, null, null, null, null);
                });

                workspaceCommand.AddCommand(requestCommand);
            }

            foreach (var macro in workspaceConfig.Macros) {
                var macroCommand = new Macro($"{workspaceName}.{macro.Key}", $"[macro] {macro.Value.Description}", macro.Value.Command);
                macroCommand.IsHidden = workspaceConfig.IsHidden;

                workspaceCommand.AddCommand(macroCommand);
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
