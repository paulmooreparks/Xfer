using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Workspace;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddXferKitWorkspaceServices(this IServiceCollection services, ISettingsService settingsService) {
        var workspaceService = new Services.Impl.WorkspaceService(settingsService.ConfigFilePath);
        services.TryAddSingleton<IWorkspaceService>(workspaceService);
        return services;
    }
}
