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
[Option(typeof(IEnumerable<string>), "--parameters", "Query parameters to include in the request. If input is redirected, parameters can also be read from standard input.", new[] { "-p" }, AllowMultipleArgumentsPerToken = true, Arity = ArgumentArity.ZeroOrMore)]
[Option(typeof(IEnumerable<string>), "--headers", "Headers to include in the request.", new[] { "-h" }, AllowMultipleArgumentsPerToken = true, Arity = ArgumentArity.ZeroOrMore)]
[Option(typeof(string), "--payload", "Content to send with the request. If input is redirected, content can also be read from standard input.", new[] { "-pl" }, Arity = ArgumentArity.ZeroOrOne)]
internal class SendCommand {
    public readonly IWorkspaceService _ws;

    public SendCommand(IWorkspaceService workspaceService) {
        _ws = workspaceService;
    }

    public async Task<int> Execute(
        [CommandParam("get")] GetCommand getCommand,
        [CommandParam("post")] PostCommand postCommand,
        [ArgumentParam("requestName")] string requestName,
        [OptionParam("--parameters")] IEnumerable<string> parameters,
        [OptionParam("--payload")] string payload,
        [OptionParam("--headers")] IEnumerable<string> headers
        ) 
    {
        if (!_ws.ActiveWorkspace.RequestDefinitions.TryGetValue(requestName, out var definition) || definition is null) { 
            Console.Error.WriteLine($"Request name '{requestName}' not found in current workspace.");
            return Result.Error;
        }

        var method = definition.Method?.ToUpper() ?? string.Empty;
        var endpoint = definition.Endpoint ?? string.Empty;

        var finalParameters = new List<string>();
        var finalHeaders = new List<string>();

        switch (method) {
            case "GET": {
                    return await getCommand.Execute(endpoint, finalParameters, finalHeaders);
                }

            case "POST": {
                    var finalPayload = payload ?? definition.Payload ?? string.Empty;
                    return await postCommand.Execute(endpoint, finalPayload, finalHeaders);
                }

            default:
                Console.Error.WriteLine($"Unknown method {method}");
                return Result.Error;
        }

        // return Result.Success;
    }
}
