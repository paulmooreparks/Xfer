using Cliffer;

using ParksComputing.Xfer.Cli.Services;
using ParksComputing.Xfer.Workspace.Services;

using System.CommandLine;
using System.CommandLine.Invocation;

namespace ParksComputing.Xfer.Cli.Commands;
[RootCommand("Xfer CLI Application")]
[Option(typeof(string), "--baseurl", "The base URL of the API to send HTTP requests to.", new[] { "-b" }, IsRequired = false)]
internal class RootCommand {
    public async Task<int> Execute(
        Command command,
        [OptionParam("--baseurl")] string baseUrl,
        IServiceProvider serviceProvider, 
        IWorkspaceService workspaceService,
        CommandSplitter splitter,
        InvocationContext context
        ) 
    {
        workspaceService.BaseUrl = baseUrl;
        return await command.Repl(
            serviceProvider, 
            context, 
            new XferReplContext(serviceProvider, workspaceService, splitter)
            );
    }
}
