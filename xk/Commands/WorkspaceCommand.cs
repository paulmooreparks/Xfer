using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Cli.Services;
using ParksComputing.XferKit.Workspace.Services;
using ParksComputing.XferKit.Scripting.Services;
using ParksComputing.XferKit.Api;

using System.CommandLine;
using System.CommandLine.Invocation;

using System.Diagnostics;
using ParksComputing.XferKit.Cli.Services.Impl;
using ParksComputing.XferKit.Cli.Repl;

namespace ParksComputing.XferKit.Cli.Commands;

internal class WorkspaceCommand {
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkspaceService _workspaceService;

    public string WorkspaceName { get; }

    public WorkspaceCommand(
        string workspaceName,
        IServiceProvider serviceProvider,
        IWorkspaceService workspaceService
        ) 
    { 
        WorkspaceName = workspaceName;
        _serviceProvider = serviceProvider;
        _workspaceService = workspaceService;
    }

    public async Task<int> Execute(
        Command command,
        InvocationContext context
        ) 
    {
        _workspaceService.SetActiveWorkspace(WorkspaceName);

        var replContext = new WorkspaceReplContext(
            new CommandSplitter(),
            _workspaceService
            );

        var result = await command.Repl(
            _serviceProvider,
            context,
            replContext
            );

        _workspaceService.SetActiveWorkspace(string.Empty);
        return result;
    }
}
