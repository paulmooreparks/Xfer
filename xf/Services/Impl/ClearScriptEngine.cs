﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

// using Jint;
// using Jint.Native;
// using Jint.Runtime.Interop;

using Microsoft.ClearScript;

using ParksComputing.Xfer.Workspace.Services;
using Newtonsoft.Json;
using ParksComputing.Xfer.Workspace.Models;
using System.Net.Http.Headers;
using Microsoft.ClearScript.V8;
using System.Dynamic;

namespace ParksComputing.Xfer.Cli.Services;

internal class ClearScriptEngine : IScriptEngine {
    private readonly IPackageService _packageService;
    private readonly IWorkspaceService _workspaceService;
    private readonly IStoreService _storeService;
    private readonly ISettingsService _settingsService;

    // private Engine _engine = new Engine(options => options.AllowClr());
    private V8ScriptEngine _engine = new V8ScriptEngine();

    public ClearScriptEngine(
        IPackageService packageService,
        IWorkspaceService workspaceService,
        IStoreService storeService,
        ISettingsService settingsService
        ) 
    {
        _workspaceService = workspaceService;
        _storeService = storeService;
        _packageService = packageService;
        _packageService.PackagesUpdated += PackagesUpdated;
        _settingsService = settingsService;
        LoadPackageAssemblies();
    }

    private void PackagesUpdated() {
        LoadPackageAssemblies();
    }

    private void LoadPackageAssemblies() {
        var assemblies = new List<Assembly>();
        var packageAssemblies = _packageService.GetInstalledPackagePaths();

        foreach (var assemblyPath in packageAssemblies) {
            try {
                var assembly = Assembly.LoadFrom(assemblyPath);
                if (assembly != null) {
                    var name = assembly.GetName().Name;
                    // Console.WriteLine($"Loaded package assembly: {assembly.FullName}");
                    assemblies.Add(assembly);
                }
            }
            catch (Exception ex) {
                Console.Error.WriteLine($"Failed to load package assembly {assemblyPath}: {ex.Message}");
            }
        }

        var langAssembly = Assembly.Load("ParksComputing.Xfer.Lang");
        assemblies.Add(langAssembly);

        InitializeScriptEnvironment(assemblies);

        // This seems like overkill. I'm doing this selectively in InitScript now.
#if false
        // Dynamically expose all public types in loaded assemblies
        foreach (var assembly in assemblies) {
            foreach (var type in assembly.GetExportedTypes()) {
                if (!type.IsGenericTypeDefinition) {
                    _engine.SetValue(type.Name, type);
                }
            }
        }
#endif

    }

    private void InitializeScriptEnvironment(IEnumerable<Assembly> assemblies) {
        // _engine = new Engine(options => options.AllowClr(assemblies.ToArray()));

        var typeCollection = new HostTypeCollection("mscorlib", "System", "System.Core");

        _engine.AddHostObject("clr", typeCollection);

        _engine.AddHostObject("console", new ConsoleScriptObject());
        _engine.AddHostObject("log", new Action<string>(Console.WriteLine));
        _engine.AddHostObject("workspaceService", _workspaceService);
        _engine.AddHostObject("store", new {
            get = new Func<string, object?>(key => _storeService.Get(key)),
            set = new Action<string, object>((key, value) => _storeService.Set(key, value)),
            delete = new Action<string>(key => _storeService.Delete(key)),
            clear = new Action(() => _storeService.Clear())
        });
        _engine.AddHostObject("btoa", new Func<string, string>(s => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(s))));
        _engine.AddHostObject("atob", new Func<string, string>(s => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(s))));

        if (_workspaceService is not null && _workspaceService.BaseConfig is not null && _workspaceService?.BaseConfig.Workspaces is not null) {
            if (_workspaceService.BaseConfig.InitScript is not null) {
                ExecuteScript(_workspaceService.BaseConfig.InitScript);
            }

            dynamic globalObject = new ExpandoObject();
            _engine.AddHostObject("xf", globalObject);
            var xfTmp = _engine.Evaluate($"xf");

            _engine.Execute(
$@"
function __preRequest(workspaceName, requestName) {{
    {GetScriptContent(_workspaceService.BaseConfig.PreRequest)}
}};

function __postRequest(workspaceName, requestName) {{
    {GetScriptContent(_workspaceService.BaseConfig.PostRequest)}
}};
");

            foreach (var workspace in _workspaceService.BaseConfig.Workspaces) {
                if (workspace.Value.InitScript is not null) {
                    ExecuteScript(workspace.Value.InitScript);
                }

                var workspaceName = workspace.Key;
                var workspaceConfig = workspace.Value;

                dynamic workspaceObj = new ExpandoObject();
                workspaceObj.name = workspaceConfig.Name ?? workspaceName;
                workspaceObj.extend = workspaceConfig.Extend;
                workspaceObj.baseWorkspace = workspaceConfig.Base ?? null;
                workspaceObj.baseUrl = workspaceConfig.BaseUrl ?? "";
                workspaceObj.requests = new ExpandoObject();
                workspaceObj.basePreRequest = _engine.Evaluate($"__preRequest");
                workspaceObj.basePostRequest = _engine.Evaluate($"__postRequest");
                workspaceObj.requests = new ExpandoObject();

                // What a hack! There must be a better way to do this.
                (globalObject as IDictionary<string, object?>)[workspaceName] = workspaceObj;
                var workspaceTmp = _engine.Evaluate($"xf.{workspaceName}");

                _engine.Execute($@"
function __preRequest__{workspaceName}(workspaceName, requestName) {{
    log('4 ' + requestName);
    {(workspaceConfig.PreRequest == null ? $"__preRequest(workspaceName, requestName)" : GetScriptContent(workspaceConfig.PreRequest))}
}};

function __postRequest__{workspaceName}(workspaceName, requestName) {{
    {(workspaceConfig.PreRequest == null ? $"__postRequest(workspaceName, requestName)" : GetScriptContent(workspaceConfig.PostRequest))}
}};

");
                // Populate requests within workspace
                foreach (var request in workspaceConfig.Requests) {
                    var requestName = request.Key;

                    var requestDef = request.Value;

                    var preRequestScript = $"{requestDef.PreRequest ?? "__preRequest__{workspaceName}(workspaceName, requestName);"}";

                    if (string.IsNullOrEmpty(requestDef.PreRequest)) {
                        preRequestScript = $"{workspaceConfig.PreRequest ?? "__preRequest(workspaceName, requestName);"}";

                        if (string.IsNullOrEmpty(workspaceConfig.PreRequest)) {
                            preRequestScript = $"{_workspaceService.BaseConfig?.PreRequest}";

                            if (string.IsNullOrEmpty(_workspaceService.BaseConfig?.PreRequest)) {
                                preRequestScript = string.Empty;
                            }
                        }
                    }

                    var postRequestScript = $"{requestDef.PostRequest ?? "__postRequest__{workspaceName}(workspaceName, requestName);"}";

                    if (string.IsNullOrEmpty(requestDef.PostRequest)) {
                        postRequestScript = $"{workspaceConfig.PostRequest ?? "__postRequest(workspaceName, requestName);"}";

                        if (string.IsNullOrEmpty(workspaceConfig.PostRequest)) {
                            postRequestScript = $"{_workspaceService.BaseConfig?.PostRequest}";

                            if (string.IsNullOrEmpty(_workspaceService.BaseConfig?.PostRequest)) {
                                postRequestScript = string.Empty;
                            }
                        }
                    }

                    _engine.Execute($@"
function __invoke__preRequest__{workspaceName}__{requestName} (workspaceName, requestName, headers, parameters, payload) {{
    let workspace = xf.{workspaceName};
    let request = xf.{workspaceName}.requests.{requestName};

    request.workspace = workspace;
    request.headers = headers;
    request.parameters = parameters;
    request.payload = payload;

    {GetScriptContent(preRequestScript)}
}}

function __invoke__postRequest__{workspaceName}__{requestName} (workspaceName, requestName, statusCode, headers, responseBody) {{
    let workspace = xf.{workspaceName};
    let request = xf.{workspaceName}.requests.{requestName};

    request.response.statusCode = statusCode;
    request.response.headers = headers;
    request.response.body = responseBody;

    {GetScriptContent(postRequestScript)}
}}

function __preRequest__{workspaceName}__{requestName} (workspaceName, requestName, headers, parameters, payload) {{
    {GetScriptContent(preRequestScript)}
}}

function __postRequest__{workspaceName}__{requestName} (workspaceName, requestName, statusCode, headers, responseBody) {{
    {GetScriptContent(postRequestScript)}
}}

");

                    dynamic requestObj = new ExpandoObject {};

                    requestObj.name = requestDef.Name ?? string.Empty;
                    requestObj.workspace = workspaceConfig;
                    requestObj.endpoint = requestDef.Endpoint ?? string.Empty;
                    requestObj.method = requestDef.Method ?? "GET";
                    requestObj.headers = requestDef.Headers ?? new Dictionary<string, string>();
                    requestObj.parameters = requestDef.Parameters ?? new List<string>();
                    requestObj.payload = requestDef.Payload ?? string.Empty;
                    requestObj.response = new ResponseDefinition();

                    requestObj.basePreRequest = new Func<object?, object?, object?>(
                        (workspace, request) => {
                                var args = new object?[] {
                                workspace,
                                request
                            };
                            var preRequestResult = _engine.Invoke(
                                $"__invoke__preRequest__{workspaceName}",
                                workspace,
                                args
                                );
                            return preRequestResult;
                        });

                    requestObj.basePostRequest = new Func<object?, object?, object?>(
                        (workspace, request) => {
                                var args = new object?[] {
                                workspace,
                                request
                            };
                            var preRequestResult = _engine.Invoke(
                                $"__postRequest__{workspaceName}",
                                workspace,
                                args
                                );
                            return preRequestResult;
                        });

                    (workspaceObj.requests as IDictionary<string, object>).Add(requestName, requestObj);
                    var requestTmp = _engine.Evaluate($"xf.{workspaceName}.requests.{requestName}");
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

        // var workspace = _workspaceService.BaseConfig!.Workspaces![workspaceName];
        // var request = workspace.Requests[requestName];

        var workspace = _engine.Evaluate($"xf['{workspaceName}']") as dynamic;
        var request = _engine.Evaluate($"xf['{workspaceName}'].requests['{requestName}'];") as dynamic;

        request.name = requestName;
        request.headers = args[2] as Dictionary<string, string> ?? [];
        request.parameters = args[3] as List<string> ?? [];
        request.payload = args[4] as string ?? null;

        var preRequestResult = _engine.Invoke(
            $"__invoke__preRequest__{workspaceName}__{requestName}",
            args
            );
    }

    public void InvokePostRequest(params object[] args) {
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

        var workspace = _engine.Evaluate($"xf['{workspaceName}']") as dynamic;
        var request = _engine.Evaluate($"xf['{workspaceName}'].requests['{requestName}'];") as dynamic;

        request.name = requestName;
        request.response.statusCode = statusCode;
        request.response.headers = headers ?? default;
        request.response.body = responseContent;

        var postRequestResult = _engine.Invoke(
            $"__invoke__postRequest__{workspaceName}__{requestName}",
            args
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
            var result = $"Error executing script: {ex.Message}";
            Console.Error.WriteLine(result);
            return result;
        }
    }

    public object Invoke(string name, object? thisObj, params object?[] value) {
        var v = _engine.Evaluate(name);
        return _engine.Invoke(name, value);
    }

    private string? GetScriptContent(string? scriptValue) {
        if (string.IsNullOrWhiteSpace(scriptValue)) {
            return string.Empty;
        }

        // Save the original working directory
        var originalDirectory = Directory.GetCurrentDirectory();
        var xferSettingsDirectory = _settingsService.XferSettingsDirectory;

        try {
            // Change to XferSettingsDirectory if it's set and exists
            // Check if the scriptValue is a file reference
            if (scriptValue.Trim().StartsWith(Constants.ScriptFilePrefix)) {
                var filePath = scriptValue.Trim().Substring(Constants.ScriptFilePrefixLength).Trim();

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
                    Console.Error.WriteLine($"⚠️ Script file not found: {filePath}");
                    return null;
                }
            }

            // If it's not a file reference, return inline script
            return scriptValue;
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"⚠️ Error processing script content: {ex.Message}");
            return null;
        }
        finally {
            // Restore the original working directory
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }
}
