using Microsoft.Extensions.DependencyInjection;

using ParksComputing.Xfer.Workspace.Services;

namespace ParksComputing.Xfer.Workspace;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddXferWorkspaceServices(this IServiceCollection services, string configFilePath) {
        // Create a single instance and register it
        var workspaceService = new Services.Impl.WorkspaceService(configFilePath);
        services.AddSingleton<IWorkspaceService>(workspaceService);

        return services;
    }
}
