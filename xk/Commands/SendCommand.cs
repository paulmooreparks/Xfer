﻿using System;
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

namespace ParksComputing.XferKit.Cli.Commands;

[Command("send", "Send a request defined in the current workspace.", IsHidden = true)]
[Argument(typeof(string), "requestName", "The name of the request to send.")]
[Option(typeof(string), "--baseurl", "The base URL of the API to send HTTP requests to.", new[] { "-b" }, IsRequired = false)]
[Option(typeof(IEnumerable<string>), "--parameters", "Query parameters to include in the request.", new[] { "-p" }, AllowMultipleArgumentsPerToken = true, Arity = ArgumentArity.ZeroOrMore)]
[Option(typeof(IEnumerable<string>), "--headers", "Headers to include in the request.", new[] { "-h" }, AllowMultipleArgumentsPerToken = true, Arity = ArgumentArity.ZeroOrMore)]
[Option(typeof(IEnumerable<string>), "--cookies", "Cookies to include in the request.", new[] { "-c" }, AllowMultipleArgumentsPerToken = true, Arity = ArgumentArity.ZeroOrMore)]
[Option(typeof(string), "--payload", "Content to send with the request. If input is redirected, content can also be read from standard input.", new[] { "-pl" }, Arity = ArgumentArity.ZeroOrOne)]
[Argument(typeof(IEnumerable<object>), "arguments", "Additional arguments passed with the request", Arity = ArgumentArity.ZeroOrMore)]
internal class SendCommand {
    private readonly IWorkspaceService _ws;
    private readonly IXferScriptEngine _scriptEngine;
    private readonly IPropertyResolver _propertyResolver;

    public SendCommand(
        IWorkspaceService workspaceService,
        IXferScriptEngine scriptEngine,
        IPropertyResolver propertyResolver
        ) 
    {
        _ws = workspaceService;
        _scriptEngine = scriptEngine;
        _propertyResolver = propertyResolver;
    }

    public async Task<int> Execute(
        [ArgumentParam("requestName")] string requestName,
        [OptionParam("--baseurl")] string? baseUrl,
        [OptionParam("--parameters")] IEnumerable<string> parameters,
        [OptionParam("--payload")] string payload,
        [OptionParam("--headers")] IEnumerable<string> headers,
        [OptionParam("--cookies")] IEnumerable<string> cookies,
        [ArgumentParam("arguments")] IEnumerable<object> arguments,
        [CommandParam("get")] GetCommand getCommand,
        [CommandParam("post")] PostCommand postCommand
        ) 
    {
        var reqSplit = requestName.Split('.');
        var workspaceName = _ws.CurrentWorkspaceName;

        if (reqSplit.Length > 1) {
            workspaceName = reqSplit[0];
            requestName = reqSplit[1];
        }

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

        if (arguments is not null && arguments.Any()) {
            using var enumerator = arguments.GetEnumerator();

            foreach (var argKvp in request.Arguments) {
                var arg = argKvp.Value;
                var argName = argKvp.Key;

                if (!enumerator.MoveNext()) {
                    break; // No more arguments to consume
                }

                var argValue = enumerator.Current;
                argsDict[argName] = argValue;
            }
        }

        if (Console.IsInputRedirected) {
            var payloadString = Console.In.ReadToEnd();
            payload = payloadString.Trim();
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

        _scriptEngine.InvokePreRequest(
            workspaceName, 
            requestName,
            configHeaders,
            finalParameters,
            payload,
            configCookies
            );


        var finalHeaders = configHeaders
            .Select(kvp => $"{kvp.Key}: {kvp.Value}")
            .ToList();

        var finalCookies = configCookies
            .Select(kvp => $"{kvp.Key}={kvp.Value}")
            .ToList();

        var result = Result.Success;

        switch (method) {
            case "GET": {
                    result = await getCommand.Execute(baseUrl, endpoint, finalParameters, finalHeaders, finalCookies, isQuiet: true);
                    _scriptEngine.InvokePostResponse(
                        workspaceName, 
                        requestName, 
                        getCommand.StatusCode, 
                        getCommand.Headers,
                        getCommand.ResponseContent
                        );
                    break;
                }

            case "POST": {
                    var finalPayload = payload ?? definition.Payload ?? string.Empty;
                    finalPayload = finalPayload.ReplaceXferKitPlaceholders(_scriptEngine, _propertyResolver, workspaceName, requestName, argsDict);
                    result = await postCommand.Execute(baseUrl, endpoint, finalPayload, finalHeaders);
                    _scriptEngine.InvokePostResponse(
                        workspaceName,
                        requestName,
                        postCommand.StatusCode,
                        postCommand.Headers,
                        postCommand.ResponseContent
                        );
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
