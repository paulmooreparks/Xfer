using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Cli.Services;

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
            await _packageService.InstallPackageAsync(install);
        }
        else if (!string.IsNullOrEmpty(uninstall)) {
            await _packageService.UninstallPackageAsync(uninstall);
        }
        else if (!string.IsNullOrEmpty(update)) {
            await _packageService.UpdatePackageAsync(update);
        }
        else if (list) {
            var plugins = _packageService.GetInstalledPackages();
            if (plugins.Count > 0) {
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
            await _packageService.SearchPackagesAsync(search);
        }
        else {
            Console.WriteLine("No command specified. Use --help for usage.");
            return Result.Error;
        }

        return Result.Success;
    }
}
