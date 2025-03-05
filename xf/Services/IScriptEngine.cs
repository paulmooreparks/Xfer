using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

using Jint;
using Jint.Native;
using Jint.Runtime.Interop;

namespace ParksComputing.Xfer.Cli.Services;

internal interface IScriptEngine {
    void SetValue(string name, object? value);
    string ExecuteScript(string? script);
    object Invoke(string name, object? thisObj, params object?[] value);
    void InvokePreRequest(params object[] args);
    void InvokePostRequest(params object[] args);
}
