using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ParksComputing.XferKit.Api.ApiMethods;

namespace ParksComputing.XferKit.Api;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddXferKitApiServices(this IServiceCollection services) {
        services.TryAddSingleton<IHttpMethods, ApiMethods.Impl.HttpMethods>();
        services.TryAddSingleton<XferKitApi>();
        return services;
    }
}
