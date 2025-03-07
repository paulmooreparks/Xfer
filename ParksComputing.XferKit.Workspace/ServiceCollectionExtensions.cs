using Microsoft.Extensions.DependencyInjection;

using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Workspace;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddXferWorkspaceServices(this IServiceCollection services, string configFilePath) {
        var workspaceService = new Services.Impl.WorkspaceService(configFilePath);
        services.AddSingleton<IWorkspaceService>(workspaceService);
        return services;
    }
}
