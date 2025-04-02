using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Api;
using ParksComputing.XferKit.Workspace;
using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Cli.Commands;

[Command("listws", "List available workspaces.", ["lw"])]
internal class ListWsCommand {
    private readonly XferKitApi _xk;
    private readonly IWorkspaceService _workspaceService;

    public ListWsCommand(
        XferKitApi xk,
        IWorkspaceService workspaceService
        ) 
    {
        _xk = xk;
        _workspaceService = workspaceService;
    }

    public int Execute() {
        if (_workspaceService.BaseConfig is null || _workspaceService.BaseConfig.Workspaces is null) {
            Console.Error.WriteLine($"{Constants.ErrorChar} No workspaces found.");
            return Result.Error;
        }

        const int padding = 20;

        foreach (var item in _workspaceService.BaseConfig.Workspaces) {
            Console.WriteLine($"  {item.Key, -padding} {item.Value.Description}");
        }

        return Result.Success;
    }
}
