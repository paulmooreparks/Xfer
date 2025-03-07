using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Cli.Commands;

[Command("setws", "Set the current workspaces.")]
[Argument(typeof(string), "workspace", "The name of the workspace to set as current.")]

internal class SetWsCommand {
    public readonly IWorkspaceService _workspaceService;

    public SetWsCommand(IWorkspaceService workspaceService) {
        _workspaceService = workspaceService;
    }

    public int Execute(
        string? workspace
        ) 
    {
        if (string.IsNullOrEmpty(workspace)) {
            var keyList = new List<string>();
            int defaultOption = 0;
            int currentOption = 0;

            foreach (var item in _workspaceService.WorkspaceList) {
                ++currentOption;
                keyList.Add(item);

                if (item == _workspaceService.CurrentWorkspaceName) {
                    defaultOption = currentOption;
                }
            }

            var keyArray = keyList.ToArray();
            var selectedItem = Services.Utility.ShowMenu(keyArray, defaultOption);

            if (selectedItem == 0) {
                return Result.Error;
            }

            workspace = keyArray[selectedItem - 1];
        }

        _workspaceService.SetActiveWorkspace(workspace);
        return Result.Success;
    }
}
