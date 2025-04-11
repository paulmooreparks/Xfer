using System.Diagnostics;
using System.Net.Sockets;
using System.Net;

namespace ParksComputing.XferKit.Plugins.Docker.Services.Impl;

internal class DockerComposeService : IDockerComposeService {
    public async Task PullAsync(string workingDirectory, string projectDirectory) {
        await RunDockerComposeCommandAsync(workingDirectory, new string[] { $"--project-directory \"{projectDirectory}\"", "pull" });
    }

    public async Task UpAsync(string workingDirectory, string projectDirectory) {
        await RunDockerComposeCommandAsync(workingDirectory, new string[] { $"--project-directory \"{projectDirectory}\"", "up -d" });
    }

    public async Task StartAsync(string workingDirectory, string projectDirectory) {
        await RunDockerComposeCommandAsync(workingDirectory, new string[] { $"--project-directory \"{projectDirectory}\"", "start" });
    }

    public async Task StopAsync(string workingDirectory, string projectDirectory) {
        await RunDockerComposeCommandAsync(workingDirectory, new string[] { $"--project-directory \"{projectDirectory}\"", "stop" });
    }

    public async Task DownAsync(string workingDirectory, string projectDirectory) {
        await RunDockerComposeCommandAsync(workingDirectory, new string[] { $"--project-directory \"{projectDirectory}\"", "down" });
    }

    public void FollowLogs(string workingDirectory, string projectDirectory) {
        var task = RunDockerComposeCommandAsync(workingDirectory, new string[] { $"--project-directory \"{projectDirectory}\"", "logs", "-f" });
        task.Wait();
    }

    public async Task ListAsync(string workingDirectory, string projectDirectory) {
        await RunDockerComposeCommandAsync(workingDirectory, new string[] { $"--project-directory \"{projectDirectory}\"", "ps" });
    }

    public async Task<string> RunDockerComposeCommandAsync(string? workingDirectory, string[] arguments, bool captureOutput = false) {
        var argumentList = new List<string>(arguments);

        var argumentsString = string.Join(' ', argumentList);

        var processStartInfo = new ProcessStartInfo {
            FileName = "docker-compose",
            Arguments = argumentsString,
            UseShellExecute = false,
            RedirectStandardOutput = captureOutput,
            CreateNoWindow = false,
            WorkingDirectory = workingDirectory,
            EnvironmentVariables = {
                ["devmachine"] = GetLocalIPAddress()
            }
        };

        if (!string.IsNullOrEmpty(workingDirectory)) {
            processStartInfo.WorkingDirectory = workingDirectory;
        }

        using var process = new Process {
            StartInfo = processStartInfo
        };

        process.Start();

        string output = "";
        if (captureOutput) {
            output = await process.StandardOutput.ReadToEndAsync();
        }

        await process.WaitForExitAsync();
        return output;
    }

    // Implement AreContainersRunning using RunDockerComposeCommandAsync to execute 'docker-compose ps'
    public async Task<bool> AreContainersRunningAsync(string workingDirectory, string projectDirectory) {
        var output = await RunDockerComposeCommandAsync(workingDirectory, new string[] { $"--project-directory \"{projectDirectory}\"", "ps" }, true);
        // Parse the output to check if any containers are running
        // For simplicity, check if the output contains more than the headers of the 'docker-compose ps' command
        return !string.IsNullOrWhiteSpace(output) && output.Split('\n').Length > 2; // Adjust the condition based on actual output parsing needs
    }

    /*
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
    */
    public static string GetLocalIPAddress() {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList) {
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
}

