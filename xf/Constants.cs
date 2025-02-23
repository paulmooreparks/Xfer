using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Cli;
internal static class Constants {
    public const string XferDirectoryName = ".xf";
    public const string ConfigFileName = "appsettings.json";
    public const string MutexName = "Global\\XferMutex"; // Global mutex name for cross-process synchronization
}
