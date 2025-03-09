using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Api;

namespace ParksComputing.XferKit.Cli.Commands;

[Command("listws", "List available workspaces.")]
internal class ListWsCommand {
    private readonly XferKitApi _xk;

    public ListWsCommand(XferKitApi xk) {
        _xk = xk;
    }

    public int Execute() {
        foreach (var item in _xk.workspaceList) {
            Console.WriteLine(item);
        }

        return Result.Success;
    }
}
