using Cliffer;

using System.CommandLine;
using System.CommandLine.Invocation;

namespace ParksComputing.Xferc;

[RootCommand("Xfer CLI Application")]
internal class RootCommand {
    public async Task<int> Execute(Command command, IServiceProvider serviceProvider, InvocationContext context) {
        return await command.Repl(serviceProvider, context, new XfercReplContext());
    }
}
