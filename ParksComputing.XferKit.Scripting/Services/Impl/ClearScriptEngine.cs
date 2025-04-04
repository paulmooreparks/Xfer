﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.ClearScript;

using ParksComputing.XferKit.Workspace.Services;
using ParksComputing.XferKit.Workspace.Models;
using System.Net.Http.Headers;
using Microsoft.ClearScript.V8;
using System.Dynamic;
using ParksComputing.XferKit.Api;
using ParksComputing.XferKit.Diagnostics.Services;

namespace ParksComputing.XferKit.Scripting.Services;

internal class ClearScriptEngine : IScriptEngine {
    private readonly IPackageService _packageService;
    private readonly IWorkspaceService _workspaceService;
    private readonly ISettingsService _settingsService;
    private readonly IAppDiagnostics<ClearScriptEngine> _diags;
    private readonly XferKitApi _xk;

    // private Engine _engine = new Engine(options => options.AllowClr());
    private V8ScriptEngine _engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);

    public ClearScriptEngine(
        IPackageService packageService,
        IWorkspaceService workspaceService,
        IStoreService storeService,
        ISettingsService settingsService,
        IAppDiagnostics<ClearScriptEngine> appDiagnostics,
        XferKitApi apiRoot
        ) 
    {
        _workspaceService = workspaceService;
        _packageService = packageService;
        _packageService.PackagesUpdated += PackagesUpdated;
        _settingsService = settingsService;
        _diags = appDiagnostics;
        _xk = apiRoot;
        var assemblies = LoadPackageAssemblies();
        InitializeScriptEnvironment(assemblies);
    }

    private void PackagesUpdated() {
        LoadPackageAssemblies();
    }

    private IEnumerable<Assembly> LoadPackageAssemblies() {
        var assemblies = new List<Assembly>();
        var packageAssemblies = _packageService.GetInstalledPackagePaths();

        foreach (var assemblyPath in packageAssemblies) {
            try {
                var assembly = Assembly.LoadFrom(assemblyPath);
                if (assembly != null) {
                    var name = assembly.GetName().Name;
                    assemblies.Add(assembly);
                }
            }
            catch (Exception ex) {
                throw new Exception($"❌ Failed to load package assembly {assemblyPath}: {ex.Message}", ex);
            }
        }

        // var langAssembly = Assembly.Load("ParksComputing.Xfer.Lang");
        // assemblies.Add(langAssembly);
        return assemblies;
    }

    private readonly Dictionary<string, dynamic> _workspaceCache = new ();
    private readonly Dictionary<string, dynamic> _requestCache = new ();

    private void InitializeScriptEnvironment(IEnumerable<Assembly> assemblies) {
        // _engine = new Engine(options => options.AllowClr(assemblies.ToArray()));
        _engine.AddHostObject("host", new ExtendedHostFunctions());

        var typeCollection = new HostTypeCollection("mscorlib", "System", "System.Core", "ParksComputing.XferKit.Workspace");

        _engine.AddHostObject("clr", typeCollection);

        _engine.AddHostType("Console", typeof(Console));
        _engine.AddHostType("console", typeof(ConsoleScriptObject));
        _engine.AddHostObject("log", new Action<string>(ConsoleScriptObject.log));
        _engine.AddHostObject("workspaceService", _workspaceService);
        _engine.AddHostObject("btoa", new Func<string, string>(s => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(s))));
        _engine.AddHostObject("atob", new Func<string, string>(s => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(s))));

        _engine.AddHostObject("xk", _xk);
        dynamic dxk = _xk;

        if (_workspaceService is not null && _workspaceService.BaseConfig is not null && _workspaceService?.BaseConfig.Workspaces is not null) {
            foreach (var kvp in _workspaceService.BaseConfig.Properties) {
                if (!_xk.TrySetProperty(kvp.Key, kvp.Value)) {
                    _diags.Emit(
                        nameof(ClearScriptEngine),
                        new {
                            Message = $"Failed to set property {kvp.Key} to {kvp.Value}"
                        }
                    );
                }
            }

            _engine.Execute(
$@"
function __preRequest(workspace, request) {{
    {GetScriptContent(_workspaceService.BaseConfig.PreRequest)}
}};

function __postResponse(workspace, request) {{
    {GetScriptContent(_workspaceService.BaseConfig.PostResponse)}
}};
");

            foreach (var workspaceKvp in _workspaceService.BaseConfig.Workspaces) {
                var workspaceName = workspaceKvp.Key;
                var workspace = workspaceKvp.Value;

                var workspaceObj = new ExpandoObject() as dynamic;
                workspaceObj.name = workspace.Name ?? workspaceName;
                workspaceObj.extend = workspace.Extend;
                workspaceObj.baseWorkspace = workspace.Base;
                workspaceObj.baseUrl = workspace.BaseUrl ?? "";
                workspaceObj.requests = new ExpandoObject() as dynamic;

                foreach (var kvp in workspace.Properties) {
                    var workspaceDict = workspaceObj as IDictionary<string, object>;
                    
                    if (workspaceDict == null) {
                        throw new InvalidOperationException("Failed to cast workspaceObj to IDictionary<string, object>");
                    }

                    if (workspaceDict.ContainsKey(kvp.Key)) {
                        _diags.Emit(
                            nameof(ClearScriptEngine),
                            new {
                                Message = $"Failed to set property {kvp.Key} to {kvp.Value} in workspace {workspaceName}"
                            }
                        );
                    }
                    else {
                        workspaceDict.Add(kvp.Key, kvp.Value);
                    }
                }

                _workspaceCache.Add(workspaceName, workspaceObj);
                (_xk.workspaces as IDictionary<string, object?>)!.Add(workspaceName, workspaceObj);

                _engine.Execute($@"
function __preRequest__{workspaceName}(workspace, request) {{
    var nextHandler = function() {{ __preRequest(workspace, request); }};
    var baseHandler = function() {{ {(string.IsNullOrEmpty(workspace.Extend) ? "" : $"__preRequest__{workspace.Extend}(workspace, request);")} }};
    {(workspace.PreRequest == null ? $"__preRequest(workspace, request)" : GetScriptContent(workspace.PreRequest))}
}};

function __postResponse__{workspaceName}(workspace, request) {{
    var nextHandler = function() {{ __postResponse(workspace, request); }};
    var baseHandler = function() {{ {(string.IsNullOrEmpty(workspace.Extend) ? "" : $"__postResponse__{workspace.Extend}(workspace, request);")} }};
    {(workspace.PostResponse == null ? $"__postResponse(workspace, request)" : GetScriptContent(workspace.PostResponse))}
}};

");
                // Populate requests within workspaceKvp
                foreach (var request in workspace.Requests) {
                    var requestName = request.Key;

                    var requestDef = request.Value;

                    _engine.Execute($@"
function __preRequest__{workspaceName}__{requestName} (workspace, request) {{
    var nextHandler = function() {{ __preRequest__{workspaceName}(workspace, request); }};
    var baseHandler = function() {{ {(string.IsNullOrEmpty(workspace.Extend) ? "" : $"__preRequest__{workspace.Extend}__{requestName}(workspace, request);")} }};
    {(requestDef.PreRequest == null ? $"__preRequest__{workspaceName}(workspace, request)" : GetScriptContent(requestDef.PreRequest))}
}}

function __postResponse__{workspaceName}__{requestName} (workspace, request) {{
    var nextHandler = function() {{ __postResponse__{workspaceName}(workspace, request); }};
    var baseHandler = function() {{ {(string.IsNullOrEmpty(workspace.Extend) ? "" : $"__postResponse__{workspace.Extend}__{requestName}(workspace, request);")} }};
    {(requestDef.PostResponse == null ? $"__postResponse__{workspaceName}(workspace, request)" : GetScriptContent(requestDef.PostResponse))}
}}

");

                    dynamic requestObj = new ExpandoObject {} as dynamic;

                    requestObj.name = requestDef.Name ?? string.Empty;
                    requestObj.endpoint = requestDef.Endpoint ?? string.Empty;
                    requestObj.method = requestDef.Method ?? "GET";
                    requestObj.headers = new ExpandoObject();
                    requestObj.parameters = requestDef.Parameters ?? new List<string>();
                    requestObj.payload = requestDef.Payload ?? string.Empty;
                    requestObj.response = new ResponseDefinition();

                    var requests = workspaceObj.requests as IDictionary<string, object>;
                    requests?.Add(requestName, requestObj);
                    _requestCache.Add($"{workspaceName}.{requestName}", requestObj);
                }
            }

            if (_workspaceService.BaseConfig.InitScript is not null) {
                ExecuteScript(_workspaceService.BaseConfig.InitScript);
            }

            foreach (var workspaceKvp in _workspaceService.BaseConfig.Workspaces) {
                var workspace = workspaceKvp.Value;

                if (workspace.InitScript is not null) {
                    ExecuteScript(workspace.InitScript);
                }
            }
        }
    }

    public void InvokePreRequest(params object[] args) {
        /*
        workspaceName = args[0]
        requestName = args[1]
        configHeaders = args[2]
        parameters = args[3]
        payload = args[4]
        */

        var workspaceName = args[0] as string ?? string.Empty;
        var requestName = args[1] as string ?? string.Empty;

        var workspace = _workspaceCache[workspaceName];
        var requests = workspace.requests as IDictionary<string, object>;
        var request = requests?[requestName] as dynamic; // _requestCache[$"{workspaceName}.{requestName}"];

        if (request is null) {
            return;
        }

        request.name = requestName;
        request.headers = new ExpandoObject() as dynamic;
        var headers = request.headers as IDictionary<string, object>;

        if (headers is null) {
            return;
        }

        var srcHeaders = args[2] as IDictionary<string, string>;

        if (srcHeaders is not null && request.headers is not null) {
            foreach (var kvp in srcHeaders) {
                headers.Add(kvp.Key, kvp.Value);
            }
        }

        var srcParameters = args[3] as IList<string>;
        var destParameters = request.parameters as IList<string>;

        if (srcParameters is not null && destParameters is not null) {
            foreach (var param in srcParameters) {
                destParameters.Add(param);
            }
        }

        var srcPayload = args[4] as string;
        request.payload = srcPayload;

        var preRequestResult = _engine.Invoke(
            $"__preRequest__{workspaceName}__{requestName}",
            workspace,
            request
            );

        // Copy headers back to original dictionary
        // I think this can be done better.
        if (srcHeaders is not null && request.headers is not null) {
            foreach (var kvp in request.headers) {
                srcHeaders[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
            }
        }

        // Copy parameters back to original list
        if (srcParameters is not null && destParameters is not null) {
            srcParameters.Clear();
            foreach (var param in destParameters) {
                srcParameters.Add(param);
            }
        }

        srcPayload = request.payload;
    }

    public void InvokePostResponse(params object?[] args) {
        /*
        workspaceName = args[0]
        requestName = args[1]
        statusCode = args[2]
        headers = args[3]
        responseContent = args[4]
        */

        var workspaceName = args[0] as string ?? string.Empty;
        var requestName = args[1] as string ?? string.Empty;
        var statusCode = args[2] as int? ?? 0;
        var headers = args[3] as HttpResponseHeaders;
        var responseContent = args[4] as string ?? string.Empty;

        var workspace = _workspaceCache[workspaceName];
        var requests = workspace.requests as IDictionary<string, object>;

        if (requests is null) {
            return;
        }

        var request = requests[requestName] as dynamic; 

        request.name = requestName;
        request.response = new ExpandoObject() as dynamic;
        request.response.statusCode = statusCode;
        request.response.headers = headers ?? default;
        request.response.body = responseContent;

        var postResponseResult = _engine.Invoke(
            $"__postResponse__{workspaceName}__{requestName}",
            workspace,
            request
            );


    }

    public void SetValue(string name, object? value) {
        _engine.AddHostObject(name, value);
    }

    public void SetResponse(ResponseDefinition dest, ResponseDefinition src) {
        dest.statusCode = src.statusCode;
        dest.body = src.body;
        dest.headers = src.headers;
    }

    public string ExecuteScript(string? script) {
        try {
            var scriptCode = GetScriptContent(script);

            if (string.IsNullOrEmpty(scriptCode)) {
                return string.Empty;
            }

            _engine.Execute(scriptCode);
            return string.Empty;
        }
        catch (Exception ex) {
            var result = $"❌ Error executing script: {ex.Message}";
            return result;
        }
    }

    public string? EvaluateScript(string? script) {
        try {
            var scriptCode = GetScriptContent(script);

            if (string.IsNullOrEmpty(scriptCode)) {
                return string.Empty;
            }

            var result = _engine.Evaluate(scriptCode);
            if (result != null && result != Undefined.Value) {
                return result.ToString() ?? string.Empty;
            }

            return null;
        }
        catch (Exception ex) {
            return $"❌ Error executing script: {ex.Message}";
        }
    }

    public string ExecuteCommand(string? script) {
        try {
            return _engine.ExecuteCommand(script);
        }
        catch (Exception ex) {
            var result = $"❌ Error executing script: {ex.Message}";
            return result;
        }
    }


    private string? GetScriptContent(string? scriptValue) {
        if (string.IsNullOrWhiteSpace(scriptValue)) {
            return string.Empty;
        }

        var originalDirectory = Directory.GetCurrentDirectory();
        var xferSettingsDirectory = _settingsService.XferSettingsDirectory;

        try {
            if (scriptValue.Trim().StartsWith(XferKit.Workspace.Constants.ScriptFilePrefix)) {
                var filePath = scriptValue.Trim().Substring(XferKit.Workspace.Constants.ScriptFilePrefixLength).Trim();

                // If the path is relative, it will now be resolved from XferSettingsDirectory
                if (!Path.IsPathRooted(filePath)) {
                    if (!string.IsNullOrWhiteSpace(xferSettingsDirectory) && Directory.Exists(xferSettingsDirectory)) {
                        Directory.SetCurrentDirectory(xferSettingsDirectory);
                    }

                    filePath = Path.GetFullPath(filePath);
                }

                if (File.Exists(filePath)) {
                    return File.ReadAllText(filePath);
                }
                else {
                    throw new FileNotFoundException($"Script file not found: {filePath}");
                }
            }

            // If it's not a file reference, return inline script
            return scriptValue;
        }
        catch (Exception ex) {
            throw new Exception($"❌ Error processing script content: {ex.Message}", ex);
        }
        finally {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}

public static class DynamicObjectExtensions {
    public static dynamic ToDynamic(object source) {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        IDictionary<string, object?> expando = new ExpandoObject();

        var properties = source.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead);

        foreach (var prop in properties) {
            var value = prop.GetValue(source);
            expando[prop.Name] = value;
        }

        return (ExpandoObject)expando;
    }
}