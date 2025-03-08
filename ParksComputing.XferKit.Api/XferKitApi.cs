using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;

using ParksComputing.XferKit.Api.ApiMethods;
using ParksComputing.XferKit.Api.Http;
using ParksComputing.XferKit.Workspace.Models;
using ParksComputing.XferKit.Workspace.Services;

// using Microsoft.ClearScript;

namespace ParksComputing.XferKit.Api;

public class XferKitApi : DynamicObject {
    private readonly Dictionary<string, object?> _properties = new();
    private readonly IWorkspaceService _workspaceService;

    public IEnumerable<string> WorkspaceList => _workspaceService.WorkspaceList;
    public string CurrentWorkspaceName => _workspaceService.CurrentWorkspaceName;

    public IHttpApi Http { get; }

    public XferKitApi(
        IWorkspaceService workspaceService, 
        IHttpApi httpApi
        ) 
    {
        _workspaceService = workspaceService;
        Http = httpApi;
    }

    public void SetActiveWorkspace(string workspaceName) => _workspaceService.SetActiveWorkspace(workspaceName);

    public WorkspaceConfig ActiveWorkspace => _workspaceService.ActiveWorkspace;

    public override bool TryGetMember(GetMemberBinder binder, out object? result) {
        return _properties.TryGetValue(binder.Name, out result);
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value) {
        _properties[binder.Name] = value;
        return true;
    }

    public override IEnumerable<string> GetDynamicMemberNames() => _properties.Keys;
}
