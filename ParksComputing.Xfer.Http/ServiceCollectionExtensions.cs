using Microsoft.Extensions.DependencyInjection;

using ParksComputing.Xfer.Http.Services;

namespace ParksComputing.Xfer.Http;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddXferHttpServices(this IServiceCollection services) {
        services.AddSingleton<IHttpService>(provider => CreateHttpService());
        return services;
    }

    private static IHttpService CreateHttpService() {
        // Instantiate the internal WorkspaceService without exposing its type
        return new Services.Impl.HttpService();
    }
}
