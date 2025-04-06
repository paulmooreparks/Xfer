using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;
using System.CommandLine;
using System.CommandLine.Invocation;

using ParksComputing.XferKit.Cli.Services;
using ParksComputing.XferKit.Workspace.Services;
using ParksComputing.XferKit.Scripting.Services;

namespace ParksComputing.XferKit.Cli.Commands;

[Command("script", "Run JavaScript interactively")]
[Argument(typeof(IEnumerable<string>), "scriptBody", "Optional script text to execute.", Arity = Cliffer.ArgumentArity.ZeroOrMore)]
internal class ScriptCommand {
    private readonly IXferScriptEngine _scriptEngine;
    private readonly IReplContext _replContext;

    public ScriptCommand(
        IXferScriptEngine scriptEngine,
        ICommandSplitter splitter,
        IWorkspaceService workspaceService
        ) 
    {
        _scriptEngine = scriptEngine;
        _replContext = new ScriptReplContext(scriptEngine, splitter, workspaceService);
    }

    public async Task<int> Execute(
        IEnumerable<string> scriptBody,
        Command command, 
        IServiceProvider serviceProvider, 
        InvocationContext context
        ) 
    {
        if (scriptBody is not null && scriptBody.Any()) {
            var script = string.Join(' ', scriptBody);
            var output = _scriptEngine.ExecuteCommand(script);
            if (!string.IsNullOrEmpty(output)) {
                Console.WriteLine(output);
            }
            return Result.Success;
        }

        return await command.Repl(serviceProvider, context, _replContext);
    }
}
