using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ParksComputing.XferKit.Diagnostics;
using ParksComputing.XferKit.Diagnostics.Services;
using ParksComputing.XferKit.Diagnostics.Services.Impl;

using ParksComputing.XferKit.Workspace.Services.Impl;
using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Workspace;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddXferKitWorkspaceServices(this IServiceCollection services) {
        services.TryAddSingleton<ISettingsService, SettingsService>();
        services.TryAddSingleton<IPackageService, PackageService>();
        services.TryAddSingleton<IStoreService, StoreService>();
        services.TryAddSingleton<IWorkspaceService, WorkspaceService>();
        return services;
    }
}
