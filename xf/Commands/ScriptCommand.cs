﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;
using System.CommandLine;
using System.CommandLine.Invocation;

using ParksComputing.Xfer.Cli.Services;
using ParksComputing.Xfer.Workspace.Services;

namespace ParksComputing.Xfer.Cli.Commands;

[Command("script", "Run JavaScript interactively")]
internal class ScriptCommand {
    private readonly IReplContext _replContext;

    public ScriptCommand(
        IScriptEngine scriptEngine,
        ICommandSplitter splitter,
        IWorkspaceService workspaceService
        ) 
    {
        _replContext = new ScriptReplContext(scriptEngine, splitter, workspaceService);
    }

    public async Task<int> Execute(
        Command command, 
        IServiceProvider serviceProvider, 
        InvocationContext context
        ) 
    {
        return await command.Repl(serviceProvider, context, _replContext);
    }
}
