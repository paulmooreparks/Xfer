using System;
using System.Collections.Generic;
using System.Net.Http;

using Jint;
using Jint.Native;
using Jint.Runtime.Interop;

namespace ParksComputing.Xfer.Cli.Services;

internal interface IScriptEngine {
    void SetGlobalVariable(string name, object? value);
    string ExecuteScript(string? script);
}
