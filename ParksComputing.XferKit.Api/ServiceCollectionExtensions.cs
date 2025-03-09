using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ParksComputing.XferKit.Api.Http;
using ParksComputing.XferKit.Api.Http.Impl;
using ParksComputing.XferKit.Api.Package;
using ParksComputing.XferKit.Api.Package.Impl;
using ParksComputing.XferKit.Api.Store;
using ParksComputing.XferKit.Api.Store.Impl;

namespace ParksComputing.XferKit.Api;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddXferKitApiServices(this IServiceCollection services) {
        services.TryAddSingleton<IHttpApi, HttpApi>();
        services.TryAddSingleton<IStoreApi, StoreApi>();
        services.TryAddSingleton<IPackageApi, PackageApi>();
        services.TryAddSingleton<XferKitApi>();
        return services;
    }
}
