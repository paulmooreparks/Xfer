using Microsoft.Extensions.DependencyInjection;

using ParksComputing.Xfer.Workspace.Services;

namespace ParksComputing.Xfer.Workspace;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddXferWorkspaceServices(this IServiceCollection services) {
        services.AddSingleton<IWorkspaceService>(provider => CreateWorkspaceService());
        return services;
    }

    private static IWorkspaceService CreateWorkspaceService() {
        // Instantiate the internal WorkspaceService without exposing its type
        return new Services.Impl.WorkspaceService();
    }
}
