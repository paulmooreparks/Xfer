using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;

namespace ParksComputing.Xfer.Cli.Services.Impl;

public class ScriptEngine
{
    private readonly Engine _engine;

    public ScriptEngine()
    {
        _engine = new Engine(options => options.AllowClr()); // Enable .NET interop
    }

    public void SetGlobalVariable(string name, object value)
    {
        _engine.SetValue(name, value);
    }

    public string ExecuteScript(string script)
    {
        try
        {
            var result = _engine.Execute(script);
            return result?.ToString() ?? string.Empty;  // .Type == Types.String ? result.AsString() : result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error executing script: {ex.Message}";
        }
    }
}
