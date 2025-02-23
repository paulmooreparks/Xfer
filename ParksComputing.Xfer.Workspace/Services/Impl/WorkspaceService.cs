using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Workspace.Services.Impl;

internal class WorkspaceService : IWorkspaceService
{
    public string? BaseUrl { get; set; }

    public WorkspaceService()
    {
        BaseUrl = string.Empty;
    }
}
