using Cliffer;

using ParksComputing.Xfer.Cli.Services;
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

    public RootCommand(
        IServiceProvider serviceProvider,
        IWorkspaceService workspaceService,
        CommandSplitter splitter,
        [OptionParam("--recursive")] Option recursionOption
        ) 
    { 
        _serviceProvider = serviceProvider;
        _workspaceService = workspaceService;
        _recursionOption = recursionOption;
        _replContext = new XferReplContext(_serviceProvider, _workspaceService, splitter, _recursionOption);
    }

    public async Task<int> Execute(
        Command command,
        System.CommandLine.RootCommand rootCommand,
        [OptionParam("--baseurl")] string? baseUrl,
        [OptionParam("--workspace")] string? workspace,
        [OptionParam("--recursive")] bool isRecursive,
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
