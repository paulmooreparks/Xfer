namespace ParksComputing.XferKit.Workspace.Services;

public interface IPackageService
{
    event Action? PackagesUpdated;

    IEnumerable<string> GetInstalledPackagePaths();
    IEnumerable<string?> GetInstalledPackages();
    Task<PackageInstallResult> InstallPackageAsync(string packageName);
    Task<PackageSearchResult> SearchPackagesAsync(string searchTerm);
    Task<PackageUninstallResult> UninstallPackageAsync(string packageName);
    Task<PackageInstallResult> UpdatePackageAsync(string packageName);
}

public class PackageInstallResult { 
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? PackageName { get; set; }
    public string? ConfirmedPackageName { get; set; }
    public string? Version { get; set; }
    public string? Path { get; set; }
}

public class PackageSearchResult {
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public IEnumerable<PackageSearchItem>? Items { get; set; }
}

public class PackageSearchItem {
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? Description { get; set; }
}

public enum PackageUninstallResult {
    Success, 
    Failed,
    NotFound
}