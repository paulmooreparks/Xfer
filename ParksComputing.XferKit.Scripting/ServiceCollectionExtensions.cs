using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ParksComputing.XferKit.Scripting.Services;
using ParksComputing.XferKit.Scripting.Services.Impl;

namespace ParksComputing.XferKit.Scripting;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddXferKitScriptingServices(this IServiceCollection services) {
        services.TryAddSingleton<IXferScriptEngine, ClearScriptEngine>();
        services.TryAddSingleton<IPropertyResolver, PropertyResolver>();
        return services;
    }
}
