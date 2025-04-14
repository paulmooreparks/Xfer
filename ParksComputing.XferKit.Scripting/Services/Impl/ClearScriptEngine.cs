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
using ParksComputing.XferKit.Workspace;

namespace ParksComputing.XferKit.Scripting.Services.Impl;

internal class ClearScriptEngine : IXferScriptEngine {
    private readonly IPackageService _packageService;
    private readonly IWorkspaceService _workspaceService;
    private readonly ISettingsService _settingsService;
    private readonly IAppDiagnostics<ClearScriptEngine> _diags;
    private readonly XferKitApi _xk;

    // private Engine _engine = new Engine(options => options.AllowClr());
    private V8ScriptEngine _engine = new V8ScriptEngine(
          V8ScriptEngineFlags.EnableDebugging 
        | V8ScriptEngineFlags.UseCaseInsensitiveMemberBinding
        | V8ScriptEngineFlags.EnableValueTaskPromiseConversion
        );

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

    public dynamic Script => _engine.Script;

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
                throw new Exception($"{Constants.ErrorChar} Failed to load package assembly {assemblyPath}: {ex.Message}", ex);
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
        _engine.AddHostType("Task", typeof(Task));
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

            try {
                _engine.Execute(
    $@"
function __preRequest(workspace, request) {{
    {GetScriptContent(_workspaceService.BaseConfig.PreRequest)}
}};
");
            }
            catch (ScriptEngineException ex) {
                Console.Error.WriteLine(ex.ErrorDetails);
            }

            var postResponseScriptContent = _workspaceService.BaseConfig.PostResponse;

            if (string.IsNullOrEmpty(_workspaceService.BaseConfig.PostResponse)) {
                postResponseScriptContent = "return request.response.body;";
            }

            try {
                _engine.Execute(
    $@"
function __postResponse(workspace, request) {{
    {postResponseScriptContent}
}};
");
            }
            catch (ScriptEngineException ex) {
                Console.Error.WriteLine(ex.ErrorDetails);
            }

            if (_workspaceService.BaseConfig.InitScript is not null) {
                try {
                    ExecuteScript(_workspaceService.BaseConfig.InitScript);
                }
                catch (ScriptEngineException ex) {
                    Console.Error.WriteLine(ex.ErrorDetails);
                }
                catch (Exception ex) {
                    Console.Error.WriteLine($"{Constants.ErrorChar} Error executing script: {ex.Message}");
                }
            }

            foreach (var workspaceKvp in _workspaceService.BaseConfig.Workspaces) {
                var workspaceName = workspaceKvp.Key;
                var workspace = workspaceKvp.Value;

                if (!string.IsNullOrEmpty(workspace.Extend) && _workspaceService.BaseConfig.Workspaces.TryGetValue(workspace.Extend, out var baseWorkspace)) {
                    workspace.Base = baseWorkspace;
                }

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

                try {
                    _engine.Execute($@"
function __preRequest__{workspaceName}(workspace, request) {{
    let nextHandler = function() {{ __preRequest(workspace, request); }};
    let baseHandler = function() {{ {(string.IsNullOrEmpty(workspace.Extend) ? "" : $"__preRequest__{workspace.Extend}(workspace, request);")} }};
    {(workspace.PreRequest == null ? $"__preRequest(workspace, request)" : GetScriptContent(workspace.PreRequest))}
}};

function __postResponse__{workspaceName}(workspace, request) {{
    let nextHandler = function() {{ return __postResponse(workspace, request); }};
    let baseHandler = function() {{ {(string.IsNullOrEmpty(workspace.Extend) ? "return null;" : $"return __postResponse__{workspace.Extend}(workspace, request);")} }};
    {(workspace.PostResponse == null ? $"return __postResponse(workspace, request)" : GetScriptContent(workspace.PostResponse))}
}};

");
                }
                catch (ScriptEngineException ex) {
                    Console.Error.WriteLine(ex.ErrorDetails);
                }

                // Populate requests within workspaceKvp
                foreach (var request in workspace.Requests) {
                    var requestName = request.Key;

                    var requestDef = request.Value;

                    var argsBuilder = new StringBuilder();

                    foreach (var arg in requestDef.Arguments) {
                        argsBuilder.Append($", {arg.Key}");
                    }

                    var extraArgs = argsBuilder.ToString();

                    try {
                        _engine.Execute($@"
function __preRequest__{workspaceName}__{requestName} (workspace, request{extraArgs}) {{
    let nextHandler = function() {{ __preRequest__{workspaceName}(workspace, request); }};
    let baseHandler = function() {{ {(string.IsNullOrEmpty(workspace.Extend) ? ";" : $"__preRequest__{workspace.Extend}__{requestName}(workspace, request{extraArgs});")} }};
    {(requestDef.PreRequest == null ? $"__preRequest__{workspaceName}(workspace, request)" : GetScriptContent(requestDef.PreRequest))}
}}

function __postResponse__{workspaceName}__{requestName} (workspace, request{extraArgs}) {{
    let nextHandler = function() {{ return __postResponse__{workspaceName}(workspace, request); }};
    let baseHandler = function() {{ {(string.IsNullOrEmpty(workspace.Extend) ? "return null;" : $"return __postResponse__{workspace.Extend}__{requestName}(workspace, request{extraArgs});")} }};
    {(requestDef.PostResponse == null ? $"return __postResponse__{workspaceName}(workspace, request)" : GetScriptContent(requestDef.PostResponse))}
}}

");
                    }
                    catch (ScriptEngineException ex) {
                        Console.Error.WriteLine(ex.ErrorDetails);
                    }

                    dynamic requestObj = new ExpandoObject {} as dynamic;

                    requestObj.name = requestDef.Name ?? string.Empty;
                    requestObj.endpoint = requestDef.Endpoint ?? string.Empty;
                    requestObj.method = requestDef.Method ?? "GET";
                    requestObj.headers = new ExpandoObject();
                    requestObj.parameters = requestDef.Parameters ?? new List<string>();
                    requestObj.payload = requestDef.Payload ?? string.Empty;
                    requestObj.response = new ResponseDefinition();

                    if (requestDef.Properties is not null) {
                        foreach (var kvp in requestDef.Properties) {
                            var requestDict = requestObj as IDictionary<string, object>;

                            if (requestDict == null) {
                                throw new InvalidOperationException("Failed to cast requestObj to IDictionary<string, object>");
                            }

                            if (requestDict.ContainsKey(kvp.Key)) {
                                _diags.Emit(
                                    nameof(ClearScriptEngine),
                                    new {
                                        Message = $"Failed to set property {kvp.Key} to {kvp.Value} in request {workspaceName}.{requestName}"
                                    }
                                );
                            }
                            else {
                                requestDict.Add(kvp.Key, kvp.Value);
                            }
                        }
                    }


                    var requests = workspaceObj.requests as IDictionary<string, object>;
                    requests?.Add(requestName, requestObj);
                    _requestCache.Add($"{workspaceName}.{requestName}", requestObj);
                }

                DefineInitScript(workspace, workspaceObj);
                CallInitScript(workspace, workspaceObj);
            }
        }
    }

    public void AddHostObject(string itemName, object target) {
        _engine.AddHostObject(itemName, HostItemFlags.None, target);
    }

    protected void DefineInitScript(WorkspaceConfig workspace, dynamic workspaceObj) {
        if (workspace.InitScript is not null) {
            var scriptCode = GetScriptContent(workspace.InitScript);
            var scriptBody = $@"function __initScript__{workspace.Name}(workspace) {{ {scriptCode} }}";
            try {
                _engine.Execute(scriptBody);
            }
            catch (ScriptEngineException ex) {
                Console.Error.WriteLine(ex.ErrorDetails);
            }
        }
    }

    protected void CallInitScript(WorkspaceConfig workspace, dynamic workspaceObj) {
        if (workspace.InitScript is not null) {
            var scriptCall = $@"__initScript__{workspace.Name}";
            Invoke(scriptCall, workspaceObj);
        }

        if (workspace.Base != null) {
            CallInitScript(workspace.Base, workspaceObj);
        }
    }

    public void InvokePreRequest(params object?[] args) {
        /*
        workspaceName = args[0]
        requestName = args[1]
        configHeaders = args[2]
        parameters = args[3]
        payload = args[4]
        cookies = args[5]
        extraArgs = args[6]
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

        var extraArgs = args[6] as IEnumerable<object>;
        var invokeArgs = new List<object> { workspace, request };

        if (extraArgs is not null) {
            invokeArgs.AddRange(extraArgs);
        }

        try {
            var preRequestResult = _engine.Invoke(
                $"__preRequest__{workspaceName}__{requestName}",
                invokeArgs.ToArray()
                );
        }
        catch (ScriptEngineException ex) {
            throw new Exception(ex.ErrorDetails);
        }

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

    public object? Invoke(string script, params object?[] args) {
        try {
            return _engine.Invoke(script, args);
        }
        catch (ScriptEngineException ex) {
            Console.Error.WriteLine(ex.ErrorDetails);
        }

        return null;
    }

    public object? InvokePostResponse(params object?[] args) {
        /*
        workspaceName = args[0]
        requestName = args[1]
        statusCode = args[2]
        headers = args[3]
        responseContent = args[4]
        extraArgs = args[5]
        */

        var workspaceName = args[0] as string ?? string.Empty;
        var requestName = args[1] as string ?? string.Empty;
        var statusCode = args[2] as int? ?? 0;
        var headers = args[3] as HttpResponseHeaders;
        var responseContent = args[4] as string ?? string.Empty;
        var extraArgs = args[5] as IEnumerable<object>;

        var workspace = _workspaceCache[workspaceName];
        var requests = workspace.requests as IDictionary<string, object>;

        if (requests is null) {
            return null;
        }

        var request = requests[requestName] as dynamic; 

        request.name = requestName;
        request.response = new ExpandoObject() as dynamic;
        request.response.statusCode = statusCode;
        request.response.headers = headers ?? default;
        request.response.body = responseContent;

        var invokeArgs = new List<object> { workspace, request };

        if (extraArgs is not null) {
            invokeArgs.AddRange(extraArgs);
        }

        try {
            var postResponseResult = _engine.Invoke(
                $"__postResponse__{workspaceName}__{requestName}",
                invokeArgs.ToArray()
                );
            return postResponseResult;
        }
        catch (ScriptEngineException ex) {
            throw new Exception(ex.ErrorDetails);
        }
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
        var scriptCode = GetScriptContent(script);

        if (string.IsNullOrEmpty(scriptCode)) {
            return string.Empty;
        }

        _engine.Execute(scriptCode);
        return string.Empty;
    }

    public object? EvaluateScript(string? script) {
        var scriptCode = GetScriptContent(script);

        if (string.IsNullOrEmpty(scriptCode)) {
            return string.Empty;
        }

        var result = _engine.Evaluate(scriptCode);

        if (result is null || result is Undefined || result is VoidResult ) {
            return null;
        }

        return result;
    }

    public string ExecuteCommand(string? script) {
        return _engine.ExecuteCommand(script);
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
            throw new Exception($"{Constants.ErrorChar} Error processing script content: {ex.Message}", ex);
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