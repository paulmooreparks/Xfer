using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xferc;
internal static class Constants {
    public const string XfercDirectoryName = ".xferc";
    public const string ConfigFileName = "appsettings.json";
    public const string MutexName = "Global\\XfercMutex"; // Global mutex name for cross-process synchronization
}
