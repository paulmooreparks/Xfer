using System;
using System.Collections.Generic;
using System.Net.Http;

using Jint;
using Jint.Native;
using Jint.Runtime.Interop;

namespace ParksComputing.Xfer.Cli.Services;

internal interface IScriptEngine {
    void SetValue(string name, object? value);
    string ExecuteScript(string? script);
    object Invoke(string name, params object?[] value);
}
