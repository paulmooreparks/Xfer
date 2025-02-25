using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.Xfer.Workspace.Services;

namespace ParksComputing.Xfer.Cli.Commands;

[Command("send", "Send a request defined in the current workspace.")]
[Argument(typeof(string), "requestName", "The name of the request to send.")]
internal class SendCommand {
    public readonly IWorkspaceService _ws;

    public SendCommand(IWorkspaceService workspaceService) {
        _ws = workspaceService;
    }

    public async Task<int> Execute(
        [CommandParam("get")] GetCommand getCommand,
        [CommandParam("post")] PostCommand postCommand,
        string requestName
        ) 
    {
        if (!_ws.ActiveWorkspace.RequestDefinitions.TryGetValue(requestName, out var definition) || definition is null) { 
            Console.Error.WriteLine($"Request name '{requestName}' not found in current workspace.");
            return Result.Error;
        }

        var method = definition.Method?.ToUpper() ?? string.Empty;
        var endpoint = definition.Endpoint ?? string.Empty;

        switch (method) {
            case "GET":
                return await getCommand.Execute(endpoint, null, null);

            case "POST":
                break;

            default:
                Console.Error.WriteLine($"Unknown method {method}");
                return Result.Error;
        }

        return Result.Success;
    }
}
