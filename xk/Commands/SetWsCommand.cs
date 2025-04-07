using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Api;

namespace ParksComputing.XferKit.Cli.Commands;

[Command("setws", "Set the current workspaces.", ["sw"])]
[Argument(typeof(string), "workspace", "The name of the workspace to set as current. Enter '/' to clear the current workspace and return to the top level.")]

internal class SetWsCommand {
    private readonly XferKitApi _xk;

    public SetWsCommand(
        XferKitApi xk
    ) 
    {
        _xk = xk;
    }

    public int Execute(
        string? workspace
        ) 
    {
        if (string.IsNullOrEmpty(workspace)) {
            var keyList = new List<string>();
            int defaultOption = 0;
            int currentOption = 0;

            foreach (var item in _xk.workspaceList) {
                ++currentOption;
                keyList.Add(item);

                if (item == _xk.currentWorkspaceName) {
                    defaultOption = currentOption;
                }
            }

            keyList.Add(".");

            var keyArray = keyList.ToArray();
            var selectedItem = Services.Utility.ShowMenu(keyArray, defaultOption);

            if (selectedItem == 0) {
                return Result.Error;
            }

            workspace = keyArray[selectedItem - 1];
        }

        _xk.setActiveWorkspace(workspace);
        return Result.Success;
    }
}
