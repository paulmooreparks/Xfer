using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.XferKit.Workspace;

public static class Constants {
    public const string XferDiagnosticsName = "XferKit";
    public const string XferDirectoryName = ".xk";
    public const string WorkspacesFileName = "workspaces.xfer";
    public const string StoreFileName = "store.sqlite";
    public const string EnvironmentFileName = ".env";
    public const string PackageDirName = "packages";
    public const string ScriptFilePrefix = "file:";
    public static int ScriptFilePrefixLength = ScriptFilePrefix.Length;
    public const string MutexName = "Global\\XferMutex"; // Global mutex name for cross-process synchronization

    public const string SuccessChar = "✅";
    public const string WarningChar = "⚠️";
    public const string ErrorChar = "❌";
}
