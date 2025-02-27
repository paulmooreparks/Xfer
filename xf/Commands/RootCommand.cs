using Cliffer;
using ParksComputing.Xfer.Cli.Services.Impl;
using ParksComputing.Xfer.Workspace.Services;

using System.CommandLine;
using System.CommandLine.Invocation;

namespace ParksComputing.Xfer.Cli.Commands;
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

    private string _currentWorkspaceName = string.Empty;

    public RootCommand(
        IServiceProvider serviceProvider,
        IWorkspaceService workspaceService,
        System.CommandLine.RootCommand rootCommand,
        CommandSplitter splitter,
        [OptionParam("--recursive")] Option recursionOption
        ) 
    { 
        _serviceProvider = serviceProvider;
        _workspaceService = workspaceService;
        _rootCommand = rootCommand;
        _recursionOption = recursionOption;
        _replContext = new XferReplContext(_serviceProvider, _workspaceService, splitter, _recursionOption);

        foreach (var workspaceKvp in _workspaceService.BaseConfig?.Workspaces ?? new Dictionary<string, Workspace.Models.WorkspaceConfig>()) {
            var workspaceName = workspaceKvp.Key;
            var workspaceConfig = workspaceKvp.Value;

            foreach (var request in workspaceConfig.Requests) {
                var macroCommand = new Macro($"{workspaceName}.{request.Key}", $"[request] {new string($"{request.Value.Method} {request.Value.Endpoint}")}", $"send {workspaceName}.{request.Key} --baseurl {workspaceKvp.Value.BaseUrl}");

                var baseurlOption = new Option<string>(["--baseurl", "-b"], "The base URL of the API to send HTTP requests to.");
                baseurlOption.IsRequired = false;
                // baseurlOption.AllowMultipleArgumentsPerToken = true;
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
