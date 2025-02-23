using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Cli.Services;
internal class WorkspaceService : IWorkspaceService {
    // public string? BaseUrl { get; set; }

    public WorkspaceService() {
        BaseUrl = string.Empty;
    }
}
