using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Workspace.Models;
using ParksComputing.XferKit.Workspace.Services;
using ParksComputing.XferKit.Scripting.Services;

using ParksComputing.XferKit.Api;
using ParksComputing.XferKit.Cli.Services;
using ParksComputing.XferKit.Workspace;
using ParksComputing.XferKit.Cli.Extensions;
using Microsoft.ClearScript;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace ParksComputing.XferKit.Cli.Commands;

public class SendCommand {
    private readonly IWorkspaceService _ws;
    private readonly IXferScriptEngine _scriptEngine;
    private readonly IPropertyResolver _propertyResolver;

    public object? CommandResult { get; private set; } = null;

    public SendCommand(
        IWorkspaceService workspaceService,
        IXferScriptEngine scriptEngine,
        IPropertyResolver? propertyResolver
        ) 
    {
        _ws = workspaceService;
        _scriptEngine = scriptEngine;

        if (propertyResolver is null) {
            throw new ArgumentNullException(nameof(propertyResolver), "Property resolver cannot be null.");
        }

        _propertyResolver = propertyResolver;
    }

    public int Handler(
        InvocationContext invocationContext,
        string workspaceName,
        string requestName,
        string? baseUrl,
        IEnumerable<string>? parameters,
        string? payload,
        IEnumerable<string>? headers,
        IEnumerable<string>? cookies,
        List<System.CommandLine.Parsing.Token>? tokenArguments,
        object?[]? objArguments,
        IClifferCli cli
        ) 
    {
        var parseResult = invocationContext.ParseResult;
        return Execute(workspaceName, requestName, baseUrl, null, null, null, null, parseResult.CommandResult.Tokens, null, cli);
    }

    public int Execute(
        string workspaceName,
        string requestName,
        string? baseUrl,
        IEnumerable<string>? parameters,
        string? payload,
        IEnumerable<string>? headers,
        IEnumerable<string>? cookies,
        IReadOnlyList<System.CommandLine.Parsing.Token>? tokenArguments,
        object?[]? objArguments,
        IClifferCli cli
        ) 
    {
        var result = DoCommand(workspaceName, requestName, baseUrl, parameters, payload, headers, cookies, tokenArguments, objArguments, cli);

        if (CommandResult is not null && !CommandResult.Equals(Undefined.Value)) {
            Console.WriteLine(CommandResult);
        }

        return result;
    }

    public int DoCommand(
        string workspaceName,
        string requestName,
        string? baseUrl,
        IEnumerable<string>? parameters,
        string? payload,
        IEnumerable<string>? headers,
        IEnumerable<string>? cookies,
        IReadOnlyList<System.CommandLine.Parsing.Token>? tokenArguments,
        object?[]? objArguments,
        IClifferCli cli
        ) 
    {
        bool isConsoleRedirected = Console.IsInputRedirected;
        var reqSplit = requestName.Split('.');

        if (_ws == null || _ws.BaseConfig == null || _ws.BaseConfig.Workspaces == null) {
            Console.Error.WriteLine($"{Constants.ErrorChar} Workspace name '{workspaceName}' not found in current configuration.");
            return Result.Error;
        }

        if (!_ws.BaseConfig.Workspaces.TryGetValue(workspaceName, out WorkspaceConfig? workspace)) {
            Console.Error.WriteLine($"{Constants.ErrorChar} Workspace name '{workspaceName}' not found in current configuration.");
            return Result.Error;
        }

        if (!workspace.Requests.TryGetValue(requestName, out var definition) || definition is null) {
            Console.Error.WriteLine($"{Constants.ErrorChar} Request name '{requestName}' not found in current workspace.");
            return Result.Error;
        }

        var request = workspace.Requests[requestName];
        var argsDict = new Dictionary<string, object?>();
        var extraArgs = new List<object?>();

        if (tokenArguments is not null && tokenArguments.Any()) {
            using var enumerator = tokenArguments.GetEnumerator();

            foreach (var argKvp in request.Arguments) {
                var arg = argKvp.Value;
                var argName = argKvp.Key;

                if (!enumerator.MoveNext()) {
                    break; // No more arguments to consume
                }

                var argValue = enumerator.Current.Value;
                argsDict[argName] = argValue;
                extraArgs.Add(argValue);
            }
        }
        else if (objArguments is not null && objArguments.Length > 0) {
            int i = 0;
            foreach (var argKvp in request.Arguments) {
                var arg = argKvp.Value;
                var argName = argKvp.Key;

                if (i >= objArguments.Length) {
                    break; // No more arguments to consume
                }

                var argValue = objArguments[i];
                argsDict[argName] = argValue;
                extraArgs.Add(argValue);
                i++;
            }
        }

        if (payload is null && isConsoleRedirected) {
            var payloadString = Console.In.ReadToEnd();
            payload = payloadString.Trim();
            isConsoleRedirected = false;
        }

        if (payload != null) {
            payload = payload.ReplaceXferKitPlaceholders(_scriptEngine, _propertyResolver, workspaceName, requestName, argsDict);
        }

        var method = definition.Method?.ToUpper() ?? string.Empty;
        var endpoint = definition.Endpoint ?? string.Empty;

        endpoint = endpoint.ReplaceXferKitPlaceholders(_scriptEngine, _propertyResolver, workspaceName, requestName, argsDict);

        var cfgParameters = definition.Parameters ?? Enumerable.Empty<string>();
        var mergedParams = new Dictionary<string, string?>();

        // Add configuration parameters first (lower precedence)
        foreach (var cfgParam in cfgParameters) {
            var parts = cfgParam.Split('=', 2);
            var key = parts[0];
            var value = parts.Length > 1 ? parts[1] : null; // Handle standalone values

            if (value is not null) {
                value = value.ReplaceXferKitPlaceholders(_scriptEngine, _propertyResolver, workspaceName, requestName, argsDict);
            }

            mergedParams.TryAdd(key, value);
        }

        // Override with command-line parameters (higher precedence)
        if (parameters is not null) {
            foreach (var parameter in parameters) {
                var parts = parameter.Split('=', 2);
                var key = parts[0];
                var value = parts.Length > 1 ? parts[1] : null;

                if (value is not null) {
                    value = value.ReplaceXferKitPlaceholders(_scriptEngine, _propertyResolver, workspaceName, requestName, argsDict);
                }

                // Always overwrite since command-line parameters take precedence
                mergedParams[key] = value;
            }
        }

        var finalParameters = mergedParams
            .Select(kvp => kvp.Value is not null ? $"{kvp.Key}={kvp.Value}" : kvp.Key)
            .ToList();

        var configHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in definition.Headers) {
            var configValue = kvp.Value?.ToString() ?? string.Empty;
            configValue = configValue.ReplaceXferKitPlaceholders(_scriptEngine, _propertyResolver, workspaceName, requestName, argsDict);
            configHeaders[kvp.Key] = configValue;
        }

        if (headers is not null) {
            foreach (var header in headers) {
                var parts = header.Split(':', 2);
                if (parts.Length == 2) {
                    var configKey = parts[0].Trim();
                    var configValue = parts[1]?.Trim() ?? string.Empty;
                    configValue = configValue.ReplaceXferKitPlaceholders(_scriptEngine, _propertyResolver, workspaceName, requestName, argsDict);
                    configHeaders[configKey] = configValue;
                }
            }
        }

        var configCookies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in definition.Cookies) {
            var configKey = kvp.Key;
            var configValue = kvp.Value ?? string.Empty;
            configValue = configValue.ReplaceXferKitPlaceholders(_scriptEngine, _propertyResolver, workspaceName, requestName, argsDict);
            configCookies[configKey] = configValue;
        }

        if (cookies is not null) {
            foreach (var cookie in cookies) {
                var parts = cookie.Split('=', 2);
                if (parts.Length == 2) {
                    var configKey = parts[0].Trim();
                    var configValue = parts[1]?.Trim() ?? string.Empty;
                    configValue = configValue.ReplaceXferKitPlaceholders(_scriptEngine, _propertyResolver, workspaceName, requestName, argsDict);
                    configCookies[configKey] = configValue;
                }
            }
        }

        try {
            _scriptEngine.InvokePreRequest(
                workspaceName,
                requestName,
                configHeaders,
                finalParameters,
                payload,
                configCookies,
                extraArgs.ToArray()
                );
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"{Constants.ErrorChar} Error executing preRequest script: {ex.Message}");
        }

        var finalHeaders = configHeaders
            .Select(kvp => $"{kvp.Key}: {kvp.Value}")
            .ToList();

        var finalCookies = configCookies
            .Select(kvp => $"{kvp.Key}={kvp.Value}")
            .ToList();

        var result = Result.Success;

        switch (method) {
            case "GET": {
                    var getCommand = cli.Commands["get"] as GetCommand;

                    if (getCommand is null) {
                        Console.Error.WriteLine($"{Constants.ErrorChar} Error: Unable to find GET command.");
                        return Result.Error;
                    }
                    result = getCommand.Execute(baseUrl, endpoint, finalParameters, finalHeaders, finalCookies, isQuiet: true);

                    try {
                        CommandResult = _scriptEngine.InvokePostResponse(
                            workspaceName,
                            requestName,
                            getCommand.StatusCode,
                            getCommand.Headers,
                            getCommand.ResponseContent,
                            extraArgs.ToArray()
                            );
                    }
                    catch (Exception ex) {
                        Console.Error.WriteLine($"{Constants.ErrorChar} Error executing preRequest script: {ex.Message}");
                    }

                    break;
                }

            case "POST": {
                    var postCommand = cli.Commands["post"] as PostCommand;

                    if (postCommand is null) {
                        Console.Error.WriteLine($"{Constants.ErrorChar} Error: Unable to find POST command.");
                        return Result.Error;
                    }

                    var finalPayload = payload ?? definition.Payload ?? string.Empty;
                    finalPayload = finalPayload.ReplaceXferKitPlaceholders(_scriptEngine, _propertyResolver, workspaceName, requestName, argsDict);
                    result = postCommand.Execute(baseUrl, endpoint, finalPayload, finalHeaders);

                    try {
                        CommandResult = _scriptEngine.InvokePostResponse(
                            workspaceName,
                            requestName,
                            postCommand.StatusCode,
                            postCommand.Headers,
                            postCommand.ResponseContent,
                            extraArgs.ToArray()
                            );
                    }
                    catch (Exception ex) {
                        Console.Error.WriteLine($"{Constants.ErrorChar} Error executing preRequest script: {ex.Message}");
                    }

                    break;
                }

            default:
                Console.Error.WriteLine($"{Constants.ErrorChar} Unknown method {method}");
                result = Result.Error;
                break;
        }

        return result;
    }
}
