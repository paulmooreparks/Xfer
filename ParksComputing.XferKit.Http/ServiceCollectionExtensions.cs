using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ParksComputing.XferKit.Diagnostics.Services;
using ParksComputing.XferKit.Diagnostics.Services.Impl;
using ParksComputing.XferKit.Http.Services;

namespace ParksComputing.XferKit.Http;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddXferKitHttpServices(this IServiceCollection services) {
        if (!services.Any(s => s.ServiceType == typeof(IHttpClientFactory))) {
            services.AddHttpClient();
        }

        if (!services.Any(s => s.ServiceType == typeof(IHttpService))) {
            services.AddHttpClient<IHttpService, Services.Impl.HttpService>();
        }

        services.AddSingleton<IAppDiagnostics<IHttpService>, AppDiagnostics<IHttpService>>();

        return services;
    }

}
