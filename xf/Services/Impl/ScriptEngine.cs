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

        // Create a new Jint engine and allow access to all loaded assemblies
        _engine = new Engine(options => options.AllowClr(assemblies.ToArray()));

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

        _engine.SetValue("console", new ConsoleBridge());
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

        if (_workspaceService.BaseConfig?.Workspaces is not null) {
            foreach (var workspace in _workspaceService.BaseConfig.Workspaces) {
                if (workspace.Value.InitScript is not null) {
                    ExecuteScript(workspace.Value.InitScript);
                }
            }
        }
    }

    public void SetGlobalVariable(string name, object? value) {
        _engine.SetValue(name, value);
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

public class ConsoleBridge {
    public void Log(params object[] args) => Console.WriteLine("[LOG] " + string.Join(" ", args));
    public void Info(params object[] args) => Console.WriteLine("[INFO] " + string.Join(" ", args));
    public void Warn(params object[] args) => Console.WriteLine("[WARN] " + string.Join(" ", args));
    public void Error(params object[] args) => Console.Error.WriteLine("[ERROR] " + string.Join(" ", args));
    public void Debug(params object[] args) => Console.WriteLine("[DEBUG] " + string.Join(" ", args));
    public void Trace(params object[] args) {
        Console.WriteLine("[TRACE] " + string.Join(" ", args));
        Console.WriteLine(Environment.StackTrace);
    }

}