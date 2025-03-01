using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Jint;
using Jint.Native;
using Jint.Runtime.Interop;

using ParksComputing.Xfer.Workspace.Services;
using Newtonsoft.Json;
using ParksComputing.Xfer.Workspace.Models;

namespace ParksComputing.Xfer.Cli.Services;

internal class ScriptEngine : IScriptEngine {
    private readonly IPackageService _packageService;
    private readonly IWorkspaceService _workspaceService;
    private readonly IStoreService _storeService;
    private readonly ISettingsService _settingsService;

    private Engine _engine = new Engine(options => options.AllowClr());

    public ScriptEngine(
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
        _engine = new Engine(options => options.AllowClr(assemblies.ToArray()));
        _engine.SetValue("console", new ConsoleScriptObject());
        _engine.SetValue("log", new Action<string>(Console.WriteLine));
        _engine.SetValue("workspace", _workspaceService);
        _engine.SetValue("store", new {
            get = new Func<string, object?>(key => _storeService.Get(key)),
            set = new Action<string, object>((key, value) => _storeService.Set(key, value)),
            delete = new Action<string>(key => _storeService.Delete(key)),
            clear = new Action(() => _storeService.Clear())
        });
        _engine.SetValue("btoa", new Func<string, string>(s => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(s))));
        _engine.SetValue("atob", new Func<string, string>(s => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(s))));

        if (_workspaceService.BaseConfig?.InitScript is not null) {
            ExecuteScript(_workspaceService.BaseConfig.InitScript);
        }

        Func<string?, string?, string?, string> preRequestScriptTemplate = (scriptBody, workspaceName, requestName) => 
        $@"(function(workspace, request) {{
    {scriptBody}
}})(
    xf['{workspaceName}'],
    xf['{workspaceName}'].requests['{requestName}']
);
";

        Func<string?, string?, string?, string> postRequestScriptTemplate = (scriptBody, workspaceName, requestName) => 
        $@"(function(workspace, request, response) {{
    {scriptBody}
}})(
    xf['{workspaceName}'],
    xf['{workspaceName}'].requests['{requestName}'], 
    xf['{workspaceName}'].requests['{requestName}'].response 
);";

        if (_workspaceService?.BaseConfig?.Workspaces is not null) {
            var globalObject = new Dictionary<string, object>();

            foreach (var workspace in _workspaceService.BaseConfig.Workspaces) {
                if (workspace.Value.InitScript is not null) {
                    ExecuteScript(workspace.Value.InitScript);
                }

                var workspaceName = workspace.Key;
                var workspaceConfig = workspace.Value;

                var workspaceObj = new Dictionary<string, object?> {
                    { "name", workspaceConfig.Name ?? workspaceName },
                    { "extend", workspaceConfig.Extend ?? workspaceName },
                    { "base", workspaceConfig.Base ?? null },
                    { "baseUrl", workspaceConfig.BaseUrl ?? "" },
                    // { "headers", workspaceConfig.Headers ?? new Dictionary<string, string>() },
                    // { "parameters", workspaceConfig.Parameters ?? new List<string>() },
                    { "requests", new Dictionary<string, object?>() } // Will hold request configs
                };

                    // Populate requests within workspace
                foreach (var request in workspaceConfig.Requests) {
                    var requestName = request.Key;
                    var requestDef = request.Value;

                    var requestObj = new Dictionary<string, object> {
                        { "name", requestDef.Name ?? string.Empty },
                        { "endpoint", requestDef.Endpoint ?? string.Empty },
                        { "method", requestDef.Method ?? "GET" },
                        { "headers", requestDef.Headers ?? new Dictionary<string, string>() },
                        { "parameters", requestDef.Parameters ?? new List<string>() },
                        { "payload", requestDef.Payload ?? string.Empty },
                        { "response", requestDef.Response },
                    };

                    requestObj["preRequest"] = new Func<object?, object?, string?>(
                            (workspace, request) => {
                                var workspaceObj = workspace as Dictionary<string, object?>;
                                var workspaceName = workspaceObj?["name"]?.ToString();
                                var requestObj = request as Dictionary<string, object?>;
                                var requestName = requestObj?["name"]?.ToString();

                                var script = preRequestScriptTemplate(
                                    requestDef.PreRequest ?? "return workspace.preRequest(workspace, request);", 
                                    workspaceName, 
                                    requestName
                                    );
                                return ExecuteScript(script);
                            });

                    requestObj["postRequest"] = new Func<object?, object?, string?>(
                            (workspace, request) => {
                                var workspaceObj = workspace as Dictionary<string, object?>;
                                var workspaceName = workspaceObj?["name"]?.ToString();
                                var requestObj = request as Dictionary<string, object?>;
                                var requestName = requestObj?["name"]?.ToString();
                                var script = postRequestScriptTemplate(
                                    requestDef.PostRequest ?? "return workspace.postRequest(workspace, request, response);", 
                                    workspaceName, 
                                    requestName
                                    );
                                return ExecuteScript(script);
                            });

                    ((Dictionary<string, object?>)workspaceObj["requests"]!)[requestName] = requestObj;
                }

                workspaceObj["preRequest"] = new Func<object?, object?, string?>(
                        (workspace, request) => {
                            var workspaceObj = workspace as Dictionary<string, object?>;
                            var workspaceName = workspaceObj?["name"]?.ToString();
                            var requestObj = request as Dictionary<string, object?>;
                            var requestName = requestObj?["name"]?.ToString();
                            var script = preRequestScriptTemplate(
                                workspaceConfig.PreRequest ?? "return xf.preRequest(workspace, request);", 
                                workspaceName, 
                                requestName
                                );
                            return ExecuteScript(script);
                        });

                workspaceObj["postRequest"] = new Func<object?, object?, string?>(
                        (workspace, request) => {
                            var workspaceObj = workspace as Dictionary<string, object?>;
                            var workspaceName = workspaceObj?["name"]?.ToString();
                            var requestObj = request as Dictionary<string, object?>;
                            var requestName = requestObj?["name"]?.ToString();
                            var script = postRequestScriptTemplate(
                                workspaceConfig.PostRequest ?? "return xf.postRequest(workspace, request, response);", 
                                workspaceName, 
                                requestName
                                );
                            return ExecuteScript(script);
                        });

                globalObject[workspaceName] = workspaceObj;
            }

            globalObject["preRequest"] = new Func<object?, object?, string?>(
                    (workspace, request) => {
                        var workspaceObj = workspace as Dictionary<string, object?>;
                        var workspaceName = workspaceObj?["name"]?.ToString();
                        var requestObj = request as Dictionary<string, object?>;
                        var requestName = requestObj?["name"]?.ToString();
                        var script = preRequestScriptTemplate(
                            _workspaceService.BaseConfig?.PreRequest ?? null, 
                            workspaceName, 
                            requestName
                            );
                        return ExecuteScript(script);
                    });

            globalObject["postRequest"] = new Func<object?, object?, string?>(
                    (workspace, request) => {
                        var workspaceObj = workspace as Dictionary<string, object?>;
                        var workspaceName = workspaceObj?["name"]?.ToString();
                        var requestObj = request as Dictionary<string, object?>;
                        var requestName = requestObj?["name"]?.ToString();
                        var script = postRequestScriptTemplate(
                            _workspaceService.BaseConfig?.PostRequest ?? null, 
                            workspaceName, 
                            requestName
                            );
                        return ExecuteScript(script);
                    });

            _engine.SetValue("xf", globalObject);
            _engine.Execute(
@"
function invokePreRequest(workspaceName, requestName, headers, parameters, payload) {
    var workspace = xf[workspaceName];
    var request = workspace.requests[requestName];

    request.headers = headers;
    request.parameters = parameters;
    request.payload = payload;

    return request.preRequest(workspace, request);
}
");
            _engine.Execute(
@"
function invokePostRequest(workspaceName, requestName, statusCode, headers, responseBody) {
    var workspace = xf[workspaceName];
    var request = workspace.requests[requestName];
    request.response.statusCode = statusCode;
    request.response.headers = headers;
    request.response.body = responseBody;
    return request.postRequest(workspace, request, request.response);
}
");

        }
    }

    public void SetValue(string name, object? value) {
        _engine.SetValue(name, value);
    }

    public void SetResponse(ResponseDefinition dest, ResponseDefinition src) {
        dest.StatusCode = src.StatusCode;
        dest.Body = src.Body;
        dest.Headers = src.Headers;
    }

    public string ExecuteScript(string? script) {
        try {
            var scriptCode = GetScriptContent(script);

            if (string.IsNullOrEmpty(scriptCode)) {
                return string.Empty;
            }

            var result = _engine.Execute(scriptCode);
            return result?.ToString() ?? string.Empty;  // .Type == Types.String ? result.AsString() : result.ToString();
        }
        catch (Exception ex) {
            var result = $"Error executing script: {ex.Message}";
            Console.Error.WriteLine(result);
            return result;
        }
    }

    public object Invoke(string name, params object?[] value) {
        return _engine.Invoke(name, value);
    }

    private string? GetScriptContent(string? scriptValue) {
        if (string.IsNullOrWhiteSpace(scriptValue)) {
            return null;
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
