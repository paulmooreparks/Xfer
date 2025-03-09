namespace ParksComputing.XferKit.Workspace.Services;

public interface IPackageService
{
    event Action? PackagesUpdated;

    List<string> GetInstalledPackagePaths();
    List<string?> GetInstalledPackages();
    Task InstallPackageAsync(string packageName);
    Task SearchPackagesAsync(string searchTerm);
    Task UninstallPackageAsync(string packageName);
    Task UpdatePackageAsync(string packageName);
}