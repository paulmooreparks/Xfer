using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Api.Package.Impl;
internal class PackageApi : IPackageApi {
    private readonly IPackageService _packageService;

    public PackageApi(
        IPackageService packageService
        ) 
    {
        _packageService = packageService;
    }

    public string[] list { 
        get {
            var list = new List<string>();

            var plugins = _packageService.GetInstalledPackages();
            if (plugins is not null && plugins.Count() > 0) {
                foreach (var plugin in plugins) {
                    list.Add($"{plugin}");
                }
            }

            return [.. list];
        }
    }

    public async Task<PackageApiResult> searchAsync(string search) {
        var result = new PackageApiResult();
        var list = new List<string>();
        var searchResult = await _packageService.SearchPackagesAsync(search);

        if (searchResult.Success == false) {
            result.success = false;
            result.message = $"Error searching for packages: {searchResult.ErrorMessage}";
        }
        else if (searchResult.Items is null || searchResult.Items.Count() == 0) {
            result.success = false;
            result.message = $"No results found for search term '{search}'.";
        }
        else {
            foreach (var package in searchResult.Items) {
                list.Add($"{package.Name} ({package.Version}) {package.Description}");
            }

            result.success = true;
            result.list = list.ToArray();
        }

        return result;
    }

    public PackageApiResult search(string search) {
        return searchAsync(search).GetAwaiter().GetResult();
    }

    public async Task<PackageApiResult> installAsync(string packageName) {
        var result = new PackageApiResult();

        var packageInstallResult = await _packageService.InstallPackageAsync(packageName);

        if (packageInstallResult == null) {
            result.success = false;
            result.message = $"Unexpected error installing package '{packageName}'.";
        }
        else if (packageInstallResult.Success) {
            result.success = true;
            result.packageName = packageInstallResult.ConfirmedPackageName;
            result.version = packageInstallResult.Version;
            result.path = packageInstallResult.Path;
            result.message = $"Installed {packageInstallResult.ConfirmedPackageName} {packageInstallResult.Version} to {packageInstallResult.Path}";
        }
        else {
            result.success = false;
            result.message = $"Failed to install package '{packageName}': {packageInstallResult.ErrorMessage}";
        }

        return result;
    }

    public PackageApiResult install(string packageName) {
        return installAsync(packageName).GetAwaiter().GetResult();
    }

    public async Task<PackageApiResult> uninstallAsync(string packageName) {
        var result = new PackageApiResult();

        var uninstallResult = await _packageService.UninstallPackageAsync(packageName);

        switch (uninstallResult) {
            case PackageUninstallResult.Success:
                result.success = true;
                result.message = $"Uninstalled {packageName}";
                break;

            case PackageUninstallResult.NotFound:
                result.success = false;
                result.message = $"Package {packageName} is not installed.";
                break;

            case PackageUninstallResult.Failed:
            default:
                result.success = false;
                result.message = $"Error uninstalling package '{packageName}'.";
                break;
        }

        return result;
    }

    public PackageApiResult uninstall(string packageName) {
        return uninstallAsync(packageName).GetAwaiter().GetResult();
    }

    public async Task<PackageApiResult> updateAsync(string packageName) {
        var result = new PackageApiResult();

        var packageInstallResult = await _packageService.UpdatePackageAsync(packageName);

        if (packageInstallResult == null) {
            result.success = false;
            result.message = $"Unexpected error updating package '{install}'.";
        }
        else if (packageInstallResult.Success) {
            result.success = true;
            result.packageName = packageInstallResult.ConfirmedPackageName;
            result.version = packageInstallResult.Version;
            result.path = packageInstallResult.Path;
            result.message = $"Updated {packageInstallResult.ConfirmedPackageName} to {packageInstallResult.Version}";
        }
        else {
            result.success = false;
            result.message = $"Failed to update package '{install}': {packageInstallResult.ErrorMessage}";
        }

        return result;
    }

    public PackageApiResult update(string packageName) {
        return updateAsync(packageName).GetAwaiter().GetResult();
    }
}
