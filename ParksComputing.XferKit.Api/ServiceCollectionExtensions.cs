using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ParksComputing.XferKit.Api.ApiMethods;
using ParksComputing.XferKit.Api.Http;
using ParksComputing.XferKit.Api.Http.Impl;

namespace ParksComputing.XferKit.Api;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddXferKitApiServices(this IServiceCollection services) {
        services.TryAddSingleton<IHttpMethods, ApiMethods.Impl.HttpMethods>();
        services.TryAddSingleton<IHttpApi, HttpApi>();
        services.TryAddSingleton<XferKitApi>();
        return services;
    }
}
