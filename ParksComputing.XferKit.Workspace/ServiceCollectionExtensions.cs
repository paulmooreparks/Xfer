using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ParksComputing.XferKit.Workspace.Services.Impl;
using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Workspace;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddXferKitWorkspaceServices(this IServiceCollection services) {
        ISettingsService settingsService = WorkspaceInitializer.InitializeWorkspace(services);
        services.TryAddSingleton<ISettingsService>(settingsService);
        var workspaceService = new Services.Impl.WorkspaceService(settingsService.ConfigFilePath);
        services.AddSingleton<IPackageService, PackageService>(provider => new PackageService(settingsService.PluginDirectory));
        services.AddSingleton<IStoreService, StoreService>(provider => new StoreService(settingsService.StoreFilePath));
        services.TryAddSingleton<IWorkspaceService>(workspaceService);
        return services;
    }
}
