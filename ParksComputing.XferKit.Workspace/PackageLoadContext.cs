using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Loader;
using System.Reflection;

namespace ParksComputing.XferKit.Workspace;

public class PackageLoadContext : AssemblyLoadContext {
    private readonly string _pluginPath;
    private readonly AssemblyDependencyResolver _resolver;

    public PackageLoadContext(string pluginPath) : base(isCollectible: true) {
        _pluginPath = pluginPath;
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName) {
        // Resolve dependencies consistently
        string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null) {
            return LoadFromAssemblyPath(assemblyPath);
        }

        // Fallback to default context for shared assemblies (e.g., Cliffer.dll)
        return null;
    }
}