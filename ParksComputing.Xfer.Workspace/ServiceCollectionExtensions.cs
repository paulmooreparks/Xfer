using Microsoft.Extensions.DependencyInjection;

using ParksComputing.Xfer.Workspace.Services;

namespace ParksComputing.Xfer.Workspace;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddXferWorkspaceServices(this IServiceCollection services, string configFilePath) {
        var workspaceService = new Services.Impl.WorkspaceService(configFilePath);
        services.AddSingleton<IWorkspaceService>(workspaceService);
        return services;
    }
}
