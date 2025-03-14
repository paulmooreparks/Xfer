using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ParksComputing.XferKit.Scripting.Services;

namespace ParksComputing.XferKit.Scripting;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddXferKitScriptingServices(this IServiceCollection services) {
        services.TryAddSingleton<IScriptEngine, ClearScriptEngine>();
        return services;
    }
}
