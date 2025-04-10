using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.XferKit.Plugins.Docker.Services;

public interface IDockerComposeService {
    Task<bool> AreContainersRunningAsync(string workingDirectory, string projectDirectory);
    Task DownAsync(string workingDirectory, string projectDirectory);
    void FollowLogs(string workingDirectory, string projectDirectory);
    Task PullAsync(string workingDirectory, string projectDirectory);
    Task StartAsync(string workingDirectory, string projectDirectory);
    Task StopAsync(string workingDirectory, string projectDirectory);
    Task UpAsync(string workingDirectory, string projectDirectory);
    Task ListAsync(string workingDirectory, string projectDirectory);
    Task<string> RunDockerComposeCommandAsync(string workingDirectory, string[] arguments, bool captureOutput = false);
}