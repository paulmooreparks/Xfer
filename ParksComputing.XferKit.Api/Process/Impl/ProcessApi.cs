using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ParksComputing.XferKit.Api.Process.Impl;
internal class ProcessApi : IProcessApi {
    public void run(string? command, string? workingDirectory, params string[]? args) {
        var arguments = string.Empty;

        if (args is not null) {
            arguments = string.Join(" ", args);
        }

        var startInfo = new ProcessStartInfo {
            FileName = command,
            Arguments = arguments,
            WorkingDirectory = workingDirectory ?? ".",
            UseShellExecute = true // Required to start a new window
        };

        var process = System.Diagnostics.Process.Start(startInfo);
    }

    public void run(string? command, string? workingDirectory) {
        run(command, workingDirectory, null);
    }
    
    public void run(string? command) {
        run(command, null, null);
    }
}
