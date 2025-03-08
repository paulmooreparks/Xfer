using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ParksComputing.XferKit.Http.Services;

namespace ParksComputing.XferKit.Http;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddXferKitHttpServices(this IServiceCollection services) {
        // services.AddHttpClient<IHttpService, Services.Impl.HttpService>();

        if (!services.Any(s => s.ServiceType == typeof(IHttpClientFactory))) {
            services.AddHttpClient();
        }

        if (!services.Any(s => s.ServiceType == typeof(IHttpService))) {
            services.AddHttpClient<IHttpService, Services.Impl.HttpService>();
        }

        return services;
    }

}
