using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Cli.Commands;

[Command("package", "Install, update, list, and remove packages.")]
[Option(typeof(string), "--install", "Install a package", ["-i"])]
[Option(typeof(string), "--uninstall", "Uninstall a package", ["-u"])]
[Option(typeof(string), "--update", "Update a package", ["-up"])]
[Option(typeof(string), "--search", "Search for packages", ["-s"])]
[Option(typeof(bool), "--list", "List installed packages", ["-l"])]
internal class PackageCommand {
    private readonly IPackageService _packageService;

    public PackageCommand(IPackageService pluginService) {
        _packageService = pluginService;
    }

    public async Task<int> Execute(
        [OptionParam("--install")] string? install,
        [OptionParam("--uninstall")] string? uninstall,
        [OptionParam("--update")] string? update,
        [OptionParam("--list")] bool list,
        [OptionParam("--search")] string? search
        ) 
    {
        if (!string.IsNullOrEmpty(install)) {
            var packageInstallResult = await _packageService.InstallPackageAsync(install);

            if (packageInstallResult == null) {
                Console.WriteLine($"Unexpected error installing package '{install}'.");
                return Result.Error;
            }

            if (packageInstallResult.Success) {
                Console.WriteLine($"✅ Installed {packageInstallResult.ConfirmedPackageName} {packageInstallResult.Version} to {packageInstallResult.Path}");
            }
            else {
                Console.WriteLine($"❌ Failed to install package '{install}': {packageInstallResult.ErrorMessage}");
                return Result.Error;
            }
        }
        else if (!string.IsNullOrEmpty(uninstall)) {
            var uninstallResult = await _packageService.UninstallPackageAsync(uninstall);

            switch (uninstallResult) {
                case Result.Success:
                    Console.WriteLine($"✅ Uninstalled {uninstall}");
                    break;

                case PackageUninstallResult.NotFound:
                    Console.Error.WriteLine($"❌ Package {uninstall} is not installed.");
                    return Result.Error;

                case PackageUninstallResult.Failed:
                default:
                    Console.Error.WriteLine($"❌ Error uninstalling package '{uninstall}'.");
                    return Result.Error;
            }

            return Result.Success;
        }
        else if (!string.IsNullOrEmpty(update)) {
            var packageInstallResult = await _packageService.UpdatePackageAsync(update);

            if (packageInstallResult == null) {
                Console.WriteLine($"Unexpected error updating package '{install}'.");
                return Result.Error;
            }

            if (packageInstallResult.Success) {
                Console.WriteLine($"✅ Updated {packageInstallResult.ConfirmedPackageName} to {packageInstallResult.Version}");
            }
            else {
                Console.WriteLine($"❌ Failed to update package '{install}': {packageInstallResult.ErrorMessage}");
                return Result.Error;
            }
        }
        else if (list) {
            var plugins = _packageService.GetInstalledPackages();
            if (plugins.Count() > 0) {
                Console.WriteLine("Installed Plugins:");
                foreach (var plugin in plugins) {
                    Console.WriteLine($"  - {plugin}");
                }
            }
            else {
                Console.WriteLine("No plugins installed.");
            }
        }
        else if (!string.IsNullOrEmpty(search)) {
            var searchResult = await _packageService.SearchPackagesAsync(search);

            if (searchResult.Success == false) {
                Console.WriteLine($"Error searching for packages: {searchResult.ErrorMessage}");
                return Result.Error;
            }

            if (searchResult.Items is null || searchResult.Items.Count() == 0) {
                Console.Error.WriteLine($"No results found for search term '{search}'.");
                return Result.Error;
            }

            Console.WriteLine($"Search results for search term '{search}':");

            foreach (var package in searchResult.Items) {
                Console.WriteLine($"  - {package.Name} ({package.Version}) {package.Description}");
            }

            // $"{package.Identity.Id} ({package.Identity.Version}) {package.Description}");

        }
        else {
            Console.WriteLine("No command specified. Use --help for usage.");
            return Result.Error;
        }

        return Result.Success;
    }
}
