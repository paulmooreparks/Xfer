﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.XferKit.Api.Process;

public interface IProcessApi {
    void run(string? command, string? workingDirectory, params string[] arguments);
    void run(string? command, string? workingDirectory);
    void run(string? command);
    string runCommand(bool captureOutput, string? workingDirectory, string command, params string[]? args);
}
