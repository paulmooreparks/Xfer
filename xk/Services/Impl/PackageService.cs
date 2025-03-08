using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace ParksComputing.XferKit.Cli.Services.Impl;

internal class PackageService : IPackageService {
    private readonly string _packageDirectory;

    private static readonly string PackageSourceUrl = "https://api.nuget.org/v3/index.json";

    public event Action? PackagesUpdated;

    public PackageService(string packageDirectory) {
        _packageDirectory = packageDirectory;
    }

    private void NotifyPackagesUpdated() {
        PackagesUpdated?.Invoke();
    }

    public async Task InstallPackageAsync(string packageName) {
        try {
            var repository = Repository.Factory.GetCoreV3(PackageSourceUrl);
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>();
            var cacheContext = new SourceCacheContext();

            // Retrieve all available versions and get the latest one
            var packageVersions = await resource.GetAllVersionsAsync(packageName, cacheContext, NullLogger.Instance, CancellationToken.None);
            var latestVersion = packageVersions?.OrderByDescending(v => v).FirstOrDefault();

            if (latestVersion == null) {
                Console.Error.WriteLine($"⚠️ No valid versions found for {packageName}");
                return;
            }

            // Retrieve package metadata to get the correct canonical name
            var metadataResource = await repository.GetResourceAsync<PackageMetadataResource>();
            var metadata = await metadataResource.GetMetadataAsync(packageName, true, true, cacheContext, NullLogger.Instance, CancellationToken.None);
            var packageMetadata = metadata.FirstOrDefault();

            if (packageMetadata == null) {
                Console.Error.WriteLine($"⚠️ Could not retrieve package metadata for {packageName}");
                return;
            }

            string confirmedPackageName = packageMetadata.Identity.Id; // Use the exact name from NuGet

            using var packageStream = new MemoryStream();
            await resource.CopyNupkgToStreamAsync(confirmedPackageName, latestVersion, packageStream, cacheContext, NullLogger.Instance, CancellationToken.None);
            packageStream.Position = 0;

            var packageReader = new PackageArchiveReader(packageStream);

            // Define framework preference order
            string[] preferredFrameworks = { "net8.0", "net7.0", "net6.0", "netstandard2.0", "net5.0" };

            IEnumerable<string> packageFiles = Enumerable.Empty<string>();

            // Find the best matching framework directory
            foreach (var framework in preferredFrameworks) {
                packageFiles = packageReader.GetFiles($"lib/{framework}");
                if (packageFiles.Any())
                    break;
            }

            if (!packageFiles.Any()) {
                Console.Error.WriteLine($"⚠️ No compatible assemblies found in {confirmedPackageName}.");
                return;
            }

            // Create the correct directory under .xf/packages/{confirmedPackageName}
            var packagePath = Path.Combine(_packageDirectory, confirmedPackageName);
            if (!Directory.Exists(packagePath)) {
                Directory.CreateDirectory(packagePath);
            }

            foreach (var file in packageFiles) {
                var destinationPath = Path.Combine(packagePath, Path.GetFileName(file));
                using var fileStream = File.Create(destinationPath);
                using var entryStream = packageReader.GetStream(file);
                await entryStream.CopyToAsync(fileStream);
            }

            Console.WriteLine($"✅ Installed {confirmedPackageName} to {packagePath}");
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"❌ Error installing {packageName}: {ex.Message}");
        }

        NotifyPackagesUpdated();
    }

    public async Task UpdatePackageAsync(string packageName) {
        await InstallPackageAsync(packageName);
    }

    public async Task UninstallPackageAsync(string packageName) {
        try {
            var packagePath = Path.Combine(_packageDirectory, packageName);
            if (Directory.Exists(packagePath)) {
                await Task.Run(() => Directory.Delete(packagePath, true));
                Console.WriteLine($"✅ Uninstalled {packageName}.");
            }
            else {
                Console.Error.WriteLine($"❌ Package {packageName} is not installed.");
            }
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"❌ Error uninstalling {packageName}: {ex.Message}");
        }

        NotifyPackagesUpdated();
    }

    private static string ExtractPackageFile(string sourcePath, string targetPath, Stream stream) {
        try {
            using var fileStream = File.Create(targetPath);
            stream.CopyTo(fileStream);
            return targetPath;
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"❌ Error extracting file {targetPath}: {ex.Message}");
            return string.Empty;
        }
    }

    public List<string?> GetInstalledPackages() {
        if (Directory.Exists(_packageDirectory)) {
            var dirs = Directory.GetDirectories(_packageDirectory);
            return dirs.Select(Path.GetFileName).ToList();
        }

        return new List<string?>();
    }

    public async Task SearchPackagesAsync(string searchTerm) {
        try {
            var packageSource = new PackageSource(PackageSourceUrl);
            var sourceRepository = Repository.Factory.GetCoreV3(packageSource);
            var searchResource = await sourceRepository.GetResourceAsync<PackageSearchResource>();

            var searchResults = await searchResource.SearchAsync(
                searchTerm,
                new SearchFilter(includePrerelease: true),
                skip: 0,
                take: 10,
                NullLogger.Instance,
                CancellationToken.None
            );

            Console.WriteLine($"Search results for '{searchTerm}':");
            foreach (var package in searchResults) {
                Console.WriteLine($"  - {package.Identity.Id} ({package.Identity.Version})");
            }
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"❌ Error searching for packages: {ex.Message}");
        }
    }

    public List<string> GetInstalledPackagePaths() {
        return Directory.EnumerateFiles(_packageDirectory, "*.dll", SearchOption.AllDirectories).ToList();
    }
}
