using System;
using System.Collections.Generic;
using System.Linq;

using ParksComputing.XferKit.Api;

namespace ParksComputing.XferKit.Scripting.Services.Impl;

public class PropertyResolver : IPropertyResolver {
    private readonly IXferScriptEngine _scriptEngine;
    private readonly XferKitApi? _base;
    private readonly IDictionary<string, dynamic?>? _workspaces;

    public PropertyResolver(IXferScriptEngine scriptEngine) {
        _scriptEngine = scriptEngine;
        _base = _scriptEngine.Script.xk as XferKitApi;
        _workspaces = _base?.workspaces as IDictionary<string, dynamic?>;
    }

    public object? ResolveProperty(
        string path,
        string? currentWorkspace = null,
        string? currentRequest = null,
        object? defaultValue = null
        ) 
    {
        if (string.IsNullOrWhiteSpace(path)) {
            return defaultValue;
        }

        string? workspacePart = null;
        string? requestPart = null;
        string? propertyPart = null;

        if (_base is null) {
            return defaultValue;
        }

        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (path.StartsWith('/')) {
            if (parts.Length == 1) {
                propertyPart = parts[0];

                if (_base.TryGetProperty(propertyPart, out object? value)) {
                    return value?.ToString() ?? defaultValue;
                }

                return defaultValue;
            }

            if (parts.Length > 1) {
                if (_workspaces is null) {
                    return defaultValue;
                }

                workspacePart = parts[0];

                if (_workspaces[workspacePart] is not IDictionary<string, dynamic?> workspace) {
                    return defaultValue;
                }

                if (parts.Length == 3) {
                    if (workspace["requests"] is not IDictionary<string, dynamic?> requests) {
                        return defaultValue;
                    }

                    requestPart = parts[1];
                    propertyPart = parts[2];

                    if (requests[requestPart] is not IDictionary<string, dynamic?> request) {
                        return defaultValue;
                    }

                    return request[propertyPart] ?? defaultValue;
                }

                propertyPart = parts[1];
                return workspace[propertyPart] ?? defaultValue;
            }

        }

        object? current = null;

        if (!string.IsNullOrEmpty(currentWorkspace) && !string.IsNullOrEmpty(currentRequest)) {
            if (_workspaces?[currentWorkspace] is not IDictionary<string, dynamic?> workspace) {
                return defaultValue;
            }

            if (workspace["requests"] is not IDictionary<string, dynamic?> requests) {
                return defaultValue;
            }

            if (requests[currentRequest] is not IDictionary<string, dynamic?> request) {
                return defaultValue; 
            }

            current = request;
        }

        foreach (var part in parts) {
            if (current is null) {
                return defaultValue;
            }

            if (part == "..") {
                if (currentRequest is not null) {
                    currentRequest = null;
                    // Move from request ➔ workspace
                    current = _workspaces?[currentWorkspace!] as IDictionary<string, dynamic?>;
                }
                else if (currentWorkspace is not null) {
                    currentWorkspace = null;
                    current = _base;
                }
                else {
                    if (_base.TryGetProperty(part, out current)) {
                        // Move from base ➔ base
                        return current ?? defaultValue;
                    }
                    else {
                        return defaultValue;
                    }
                }
            }
            else {
                current = TryGetProperty(current, part) ?? defaultValue;
            }
        }

        return current ?? defaultValue;
    }

    public T? ResolveProperty<T>(string path, string? currentWorkspace = null, string? currentRequest = null, T? defaultValue = default) {
        var result = ResolveProperty(path, currentWorkspace, currentRequest);

        if (result is null) {
            return defaultValue;
        }

        try {
            return result is T typed
                ? typed
                : (T)Convert.ChangeType(result, typeof(T));
        }
        catch {
            return defaultValue;
        }
    }

    private object? TryGetProperty(object current, string propertyName) {
        if (current is IDictionary<string, dynamic?> dict) {
            dict.TryGetValue(propertyName, out var value);
            return value;
        }

        if (current is XferKitApi xk) {
            xk.TryGetProperty(propertyName, out var value);
            return value;
        }

        return null;
    }
}
