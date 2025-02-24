using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.Xfer.Workspace.Services;

namespace ParksComputing.Xfer.Cli.Commands;

[Command("listws", "List available workspaces.")]
internal class ListWsCommand {
    public readonly IWorkspaceService _workspaceService;

    public ListWsCommand(IWorkspaceService workspaceService) {
        _workspaceService = workspaceService ?? throw new ArgumentNullException(nameof(workspaceService));
    }

    public int Execute() {
        if (_workspaceService != null) {
            foreach (var item in _workspaceService.WorkspaceList) {
                Console.WriteLine(item);
            }
        }

        return Result.Success;
    }
}
