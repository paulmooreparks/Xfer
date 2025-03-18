using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Api;
using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Cli.Commands;

[Command("package", "Install, update, list, and remove packages.")]
[Option(typeof(string), "--install", "Install a package", ["-i"])]
[Option(typeof(string), "--uninstall", "Uninstall a package", ["-u"])]
[Option(typeof(string), "--update", "Update a package", ["-up"])]
[Option(typeof(string), "--search", "Search for packages", ["-s"])]
[Option(typeof(bool), "--list", "List installed packages", ["-l"])]
internal class PackageCommand {
    private readonly XferKitApi _xk;

    public PackageCommand(XferKitApi xferKitApi) {
        _xk = xferKitApi;
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
            var packageInstallResult = await _xk.package.installAsync(install);

            if (packageInstallResult == null) {
                Console.Error.WriteLine($"❌ Unexpected error installing package '{install}'.");
                return Result.Error;
            }

            if (packageInstallResult.success) {
                Console.WriteLine($"✅ Installed {packageInstallResult.packageName} {packageInstallResult.version} to {packageInstallResult.path}");
            }
            else {
                Console.Error.WriteLine($"❌ Failed to install package '{install}': {packageInstallResult.message}");
                return Result.Error;
            }
        }
        else if (!string.IsNullOrEmpty(uninstall)) {
            var uninstallResult = await _xk.package.uninstallAsync(uninstall);

            if (uninstallResult == null) {
                Console.Error.WriteLine($"❌ Unexpected error uninstalling package '{uninstall}'.");
                return Result.Error;
            }

            if (uninstallResult.success) {
                Console.WriteLine($"✅ {uninstallResult.message}");
            }
            else {
                Console.Error.WriteLine($"❌ {uninstallResult.message}");
                return Result.Error;
            }

            return Result.Success;
        }
        else if (!string.IsNullOrEmpty(update)) {
            var packageInstallResult = await _xk.package.updateAsync(update);

            if (packageInstallResult == null) {
                Console.Error.WriteLine($"❌ Unexpected error updating package '{install}'.");
                return Result.Error;
            }

            if (packageInstallResult.success) {
                Console.WriteLine($"✅ {packageInstallResult.message}");
            }
            else {
                Console.Error.WriteLine($"❌ {packageInstallResult.message}");
                return Result.Error;
            }
        }
        else if (list) {
            var plugins = _xk.package.list;

            if (plugins.Count() > 0) {
                Console.WriteLine("Installed Plugins:");
                foreach (var plugin in plugins) {
                    Console.WriteLine($"  - {plugin}");
                }
            }
            else {
                Console.WriteLine("⚠️ No plugins installed.");
            }
        }
        else if (!string.IsNullOrEmpty(search)) {
            var searchResult = await _xk.package.searchAsync(search);

            if (searchResult == null) {
                Console.Error.WriteLine($"❌ Unexpected error searching for package '{search}'.");
                return Result.Error;
            }

            if (searchResult.success == false) {
                Console.Error.WriteLine($"❌ Error searching for packages: {searchResult.message}");
                return Result.Error;
            }

            if (searchResult.list is null || searchResult.list.Count() == 0) {
                Console.Error.WriteLine($"❌ No results found for search term '{search}'.");
                return Result.Error;
            }

            Console.WriteLine($"Search results for search term '{search}':");

            foreach (var package in searchResult.list) {
                Console.WriteLine($"  - {package}");
            }
        }
        else {
            Console.Error.WriteLine("❌ No command specified. Use --help for usage.");
            return Result.Error;
        }

        return Result.Success;
    }
}
