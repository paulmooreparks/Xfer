using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using ParksComputing.XferKit.Plugins.Docker.Services;

namespace ParksComputing.XferKit.Plugins.Docker.Commands;

[Command("docker-compose", "Manage Abacus Docker containers via docker-compose commands")]
[Option(typeof(string), "--project-directory", "Specify an alternate working directory", ["-pd"], Arity = ArgumentArity.ZeroOrOne)]
[Option(typeof(bool), "--help", "Show help information", ["-?", "-h", "/?", "?"], Arity = ArgumentArity.ZeroOrOne)]
[Argument(typeof(IEnumerable<string>), "arguments", "Additional options to the docker-compose command", Arity = ArgumentArity.ZeroOrMore)]
internal class DockerComposeCommand {
    public async Task<int> Execute(
        [OptionParam("--help")] bool help,
        [OptionParam("--project-directory")] string projectDirectory,
        IEnumerable<string> arguments,
        System.CommandLine.Command cliCommand,
        InvocationContext context,
        // IAbacusDevConfigurationService config,
        IDockerComposeService docker
        ) 
    {
        if (arguments.Count() > 0) {
            var argsList = new List<string>();

            if (!string.IsNullOrEmpty(projectDirectory)) {
                argsList.Add("--project-directory");
                argsList.Add($"\"{projectDirectory}\"");
            }

            argsList.AddRange(arguments);

            if (help) {
                argsList.Add("--help");
            }

            var optionsList = new List<string>();

            foreach (var arg in arguments) {
                optionsList.Add(arg);
            }

            var args = argsList.ToArray();
            var commandArgs = optionsList.ToArray();
            await docker.RunDockerComposeCommandAsync(projectDirectory, args);
            return Result.Success;
        }

        if (help) {
            await docker.RunDockerComposeCommandAsync(projectDirectory, new string[] { "--help" });
            return Result.Success;
        }

        return Result.Success;
    }
}
