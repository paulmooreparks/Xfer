﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ParksComputing.XferKit.Scripting.Services;

public interface IScriptEngine {
    void SetValue(string name, object? value);
    string ExecuteScript(string? script);
    string? EvaluateScript(string? script);
    string ExecuteCommand(string? script);
    void InvokePreRequest(params object[] args);
    void InvokePostResponse(params object?[] args);
}
