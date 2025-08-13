using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Json2Xfer;

internal static class Constants {
    public const string XfercDirectoryName = ".json2xfer";
    public const string ConfigFileName = "appsettings.json";
    public const string MutexName = "Global\\Json2XferMutex"; // Global mutex name for cross-process synchronization
}
