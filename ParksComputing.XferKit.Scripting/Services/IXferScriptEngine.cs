using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ParksComputing.XferKit.Scripting.Services;

public interface IXferScriptEngine {
    public dynamic Script { get; }
    void SetValue(string name, object? value);
    string ExecuteScript(string? script);
    object? EvaluateScript(string? script);
    string ExecuteCommand(string? script);
    void InvokePreRequest(params object?[] args);
    object? InvokePostResponse(params object?[] args);
    object? Invoke(string script, params object?[] args);
    void AddHostObject(string itemName, object target);
}
