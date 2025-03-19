using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;

using ParksComputing.XferKit.Diagnostics.Services;
using ParksComputing.XferKit.Diagnostics.Services.Impl;

namespace ParksComputing.XferKit.Diagnostics;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddXferKitDiagnosticsServices(this IServiceCollection services, string name) {
        services.AddSingleton<DiagnosticSource>(new DiagnosticListener(name));
        services.AddSingleton(typeof(IAppDiagnostics<>), typeof(AppDiagnostics<>));
        return services;
    }
}
