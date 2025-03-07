using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Cli.Commands;

[Command("listws", "List available workspaces.")]
internal class ListWsCommand {
    public readonly IWorkspaceService _workspaceService;

    public ListWsCommand(IWorkspaceService workspaceService) {
        _workspaceService = workspaceService;
    }

    public int Execute() {
        foreach (var item in _workspaceService.WorkspaceList) {
            Console.WriteLine(item);
        }

        return Result.Success;
    }
}
