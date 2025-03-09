using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;

using ParksComputing.XferKit.Api.Http;
using ParksComputing.XferKit.Api.Package;
using ParksComputing.XferKit.Api.Store;
using ParksComputing.XferKit.Workspace.Models;
using ParksComputing.XferKit.Workspace.Services;

// using Microsoft.ClearScript;

namespace ParksComputing.XferKit.Api;

public class XferKitApi : DynamicObject {
    private readonly Dictionary<string, object?> _properties = new();
    private readonly IWorkspaceService _workspaceService;

    public IEnumerable<string> workspaceList => _workspaceService.WorkspaceList;
    public string currentWorkspaceName => _workspaceService.CurrentWorkspaceName;

    public IHttpApi http { get; }

    public IStoreApi store { get; }

    public IPackageApi package { get; }

    public XferKitApi(
        IWorkspaceService workspaceService, 
        IHttpApi httpApi,
        IStoreApi storeApi,
        IPackageApi packageApi
        ) 
    {
        _workspaceService = workspaceService;
        http = httpApi;
        store = storeApi;
        package = packageApi;
    }

    public void setActiveWorkspace(string workspaceName) => _workspaceService.SetActiveWorkspace(workspaceName);

    public WorkspaceConfig activeWorkspace => _workspaceService.ActiveWorkspace;

    public string[] workspaces => _workspaceService.WorkspaceList.ToArray();

    public override bool TryGetMember(GetMemberBinder binder, out object? result) {
        return _properties.TryGetValue(binder.Name, out result);
    }

    public bool tryGetMember(GetMemberBinder binder, out object? result) => TryGetMember(binder, out result);

    public override bool TrySetMember(SetMemberBinder binder, object? value) {
        _properties[binder.Name] = value;
        return true;
    }

    public bool trySetMember(SetMemberBinder binder, object? value) => TrySetMember(binder, value);

    public override IEnumerable<string> GetDynamicMemberNames() => _properties.Keys;

    public IEnumerable<string> getDynamicMemberNames() => GetDynamicMemberNames();
}
