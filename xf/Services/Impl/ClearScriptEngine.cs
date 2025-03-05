using System;
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
function __preRequest(workspace, request) {{
    {GetScriptContent(_workspaceService.BaseConfig.PreRequest)}
}};

function __postRequest(workspace, request) {{
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
function __preRequest__{workspaceName}(workspace, request) {{
    {(workspaceConfig.PreRequest == null ? $"__preRequest(workspace, request)" : GetScriptContent(workspaceConfig.PreRequest))}
}};

function __postRequest__{workspaceName}(workspace, request) {{
    {(workspaceConfig.PreRequest == null ? $"__postRequest(workspace, request)" : GetScriptContent(workspaceConfig.PostRequest))}
}};

");
                // Populate requests within workspace
                foreach (var request in workspaceConfig.Requests) {
                    var requestName = request.Key;

                    var requestDef = request.Value;

                    _engine.Execute($@"
function __preRequest__{workspaceName}__{requestName} (workspace, request) {{
    // let workspace = xf.{workspaceName};
    // let request = xf.{workspaceName}.requests.{requestName};

    {(requestDef.PreRequest == null ? $"__preRequest__{workspaceName}(workspace, request)" : GetScriptContent(requestDef.PreRequest))}
}}

function __postRequest__{workspaceName}__{requestName} (workspace, request) {{
    // let workspace = xf.{workspaceName};
    // let request = xf.{workspaceName}.requests.{requestName};

    {(requestDef.PostRequest == null ? $"__postRequest__{workspaceName}(workspace, request)" : GetScriptContent(requestDef.PostRequest))}
}}

");

                    dynamic requestObj = new ExpandoObject {};

                    requestObj.name = requestDef.Name ?? string.Empty;
                    requestObj.workspace = workspaceConfig;
                    requestObj.endpoint = requestDef.Endpoint ?? string.Empty;
                    requestObj.method = requestDef.Method ?? "GET";
                    requestObj.headers = new ExpandoObject();
                    requestObj.parameters = requestDef.Parameters ?? new List<string>();
                    requestObj.payload = requestDef.Payload ?? string.Empty;
                    requestObj.response = new ResponseDefinition();

                    requestObj.basePreRequest = new Func<object?>(
                        () => {
                            var workspace = _engine.Evaluate($"xf.{workspaceName}");
                            var request = _engine.Evaluate($"xf.{workspaceName}.requests.{requestName}");
                            var fn = $"__preRequest__{workspaceName}";

                            var result = _engine.Invoke(
                                fn,
                                workspace,
                                request
                                );
                            return result;
                        });

                    requestObj.basePostRequest = new Func<object?>(
                        () => {
                            var workspace = _engine.Evaluate($"xf.{workspaceName}");
                            var request = _engine.Evaluate($"xf.{workspaceName}.requests.{requestName}");
                            var fn = $"__postRequest__{workspaceName}";

                            var result = _engine.Invoke(
                                fn,
                                workspace,
                                request
                                );
                            return result;
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
        request.workspace = workspace;
        // request.headers = args[2] as Dictionary<string, string> ?? [];
        request.headers = new ExpandoObject() as dynamic;

        var srcHeaders = args[2] as IDictionary<string, string>;
        var destHeaders = request.headers as IDictionary<string, object>;

        if (srcHeaders is not null && destHeaders is not null) {
            foreach (var kvp in srcHeaders) {
                destHeaders.Add(kvp.Key, kvp.Value);
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
        if (srcHeaders is not null && destHeaders is not null) {
            foreach (var kvp in destHeaders) {
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
        request.response = new ExpandoObject() as dynamic;
        request.response.statusCode = statusCode;
        request.response.headers = headers ?? default;
        request.response.body = responseContent;

        var postRequestResult = _engine.Invoke(
            $"__postRequest__{workspaceName}__{requestName}",
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
