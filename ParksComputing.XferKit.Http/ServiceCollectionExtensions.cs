using Microsoft.Extensions.DependencyInjection;

using ParksComputing.XferKit.Http.Services;

namespace ParksComputing.XferKit.Http;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddXferHttpServices(this IServiceCollection services) {
        services.AddHttpClient<IHttpService, Services.Impl.HttpService>();
        return services;
    }
}
