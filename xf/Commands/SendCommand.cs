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

        var cfgParameters = definition.Parameters ?? Enumerable.Empty<string>();
        var mergedParams = new Dictionary<string, string?>();

        // Add configuration parameters first (lower precedence)
        foreach (var cfgParam in cfgParameters) {
            var parts = cfgParam.Split('=', 2);
            var key = parts[0];
            var value = parts.Length > 1 ? parts[1] : null; // Handle standalone values

            if (!mergedParams.ContainsKey(key)) // Avoid overwriting with lower-priority values
            {
                mergedParams[key] = value;
            }
        }

        // Override with command-line parameters (higher precedence)
        if (parameters is not null) {
            foreach (var parameter in parameters) {
                var parts = parameter.Split('=', 2);
                var key = parts[0];
                var value = parts.Length > 1 ? parts[1] : null;

                // Always overwrite since command-line parameters take precedence
                mergedParams[key] = value;
            }
        }

        // Convert back to List<string>
        var finalParameters = mergedParams
            .Select(kvp => kvp.Value is not null ? $"{kvp.Key}={kvp.Value}" : kvp.Key)
            .ToList();

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
