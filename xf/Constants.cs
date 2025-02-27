using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Cli;
internal static class Constants {
    public const string XferDirectoryName = ".xf";
    public const string WorkspacesFileName = "workspaces.xfer";
    public const string StoreFileName = "store.xfer";
    public const string EnvironmentFileName = ".env";
    public const string PackageDirName = "packages";
    public const string ScriptFilePrefix = "file:";
    public static int ScriptFilePrefixLength = ScriptFilePrefix.Length;
    public const string MutexName = "Global\\XferMutex"; // Global mutex name for cross-process synchronization
}
