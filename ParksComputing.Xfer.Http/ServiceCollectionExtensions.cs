using Microsoft.Extensions.DependencyInjection;

using ParksComputing.Xfer.Http.Services;

namespace ParksComputing.Xfer.Http;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddXferHttpServices(this IServiceCollection services) {
        services.AddHttpClient<IHttpService, Services.Impl.HttpService>();
        return services;
    }
}
