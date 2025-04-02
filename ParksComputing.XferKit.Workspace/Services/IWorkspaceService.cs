using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParksComputing.XferKit.Workspace.Models;

namespace ParksComputing.XferKit.Workspace.Services;

public interface IWorkspaceService {
    BaseConfig BaseConfig { get; }
    IEnumerable<string> WorkspaceList { get; }
    WorkspaceConfig ActiveWorkspace { get; }
    string CurrentWorkspaceName { get; }
    void SetActiveWorkspace(string workspaceName);
}
