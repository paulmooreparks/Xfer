using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParksComputing.Xfer.Workspace.Models;

namespace ParksComputing.Xfer.Workspace.Services;

public interface IWorkspaceService {
    BaseConfig? BaseConfig { get; }
    IEnumerable<string> WorkspaceList { get; }
    WorkspaceConfig ActiveWorkspace { get; }
    string CurrentWorkspaceName { get; }
    void SetActiveWorkspace(string workspaceName);
}
