using Cliffer;

using ParksComputing.Xfer.Cli.Services;

using System.CommandLine;
using System.CommandLine.Invocation;

namespace ParksComputing.Xfer.Cli.Commands;
[RootCommand("Xfer CLI Application")]
[Option(typeof(string), "--baseUrl", "The base URL of the API to send HTTP requests to.", new[] { "-b" }, IsRequired = false)]
internal class RootCommand {
    public async Task<int> Execute(
        Command command,
        [OptionParam("--baseUrl")] string baseUrl,
        IServiceProvider serviceProvider, 
        IWorkspaceService workspaceService,
        InvocationContext context
        ) 
    {
        workspaceService.BaseUrl = baseUrl;
        return await command.Repl(
            serviceProvider, 
            context, 
            new XferReplContext(serviceProvider, workspaceService)
            );
    }
}
