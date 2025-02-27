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

    private Engine _engine = new Engine(options => options.AllowClr());

    public ScriptEngine(
        IPackageService packageService,
        IWorkspaceService workspaceService,
        IStoreService storeService
        ) 
    {
        _workspaceService = workspaceService;
        _storeService = storeService;
        _packageService = packageService;
        _packageService.PackagesUpdated += PackagesUpdated;
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

    public void SetGlobalVariable(string name, object value) {
        _engine.SetValue(name, value);
    }

    public string ExecuteScript(string script) {
        try {
            var result = _engine.Execute(script);
            return result?.ToString() ?? string.Empty;  // .Type == Types.String ? result.AsString() : result.ToString();
        }
        catch (Exception ex) {
            var result = $"Error executing script: {ex.Message}";
            Console.Error.WriteLine(result);
            return result;
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