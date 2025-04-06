using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.XferKit.Scripting.Services;

public interface IPropertyResolver {
    object? ResolveProperty(string path, string? currentWorkspace = null, string? currentRequest = null, object? defaultValue = null);
    T? ResolveProperty<T>(string path, string? currentWorkspace = null, string? currentRequest = null, T? defaultValue = default);
}
