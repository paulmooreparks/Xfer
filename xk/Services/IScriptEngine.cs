using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ParksComputing.XferKit.Cli.Services;

internal interface IScriptEngine {
    void SetValue(string name, object? value);
    string ExecuteScript(string? script);
    void InvokePreRequest(params object[] args);
    void InvokePostRequest(params object[] args);
}
