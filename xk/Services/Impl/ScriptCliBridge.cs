using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using Microsoft.ClearScript;

namespace ParksComputing.XferKit.Cli.Services.Impl;
public class ScriptCliBridge : IScriptCliBridge {
    public System.CommandLine.RootCommand? RootCommand { get; set; } = default;

    public ScriptCliBridge() {
    }

    [ScriptMember("runCommand")]
    public int RunCommand(string commandName, params object?[] args) {
        Console.WriteLine(commandName);
        return Result.Success;
    }
}
