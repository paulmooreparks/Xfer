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

namespace ParksComputing.XferKit.Workspace.Services.Impl;

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

    public async Task<PackageInstallResult> InstallPackageAsync(string packageName) {
        var result = new PackageInstallResult { PackageName = packageName };

        try {
            var repository = Repository.Factory.GetCoreV3(PackageSourceUrl);
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>();
            var cacheContext = new SourceCacheContext();

            // Retrieve all available versions and get the latest one
            var packageVersions = await resource.GetAllVersionsAsync(packageName, cacheContext, NullLogger.Instance, CancellationToken.None);
            var latestVersion = packageVersions?.OrderByDescending(v => v).FirstOrDefault();

            if (latestVersion == null) {
                result.ErrorMessage = $"No valid versions found for {packageName}";
                result.Success = false;
                return result;
            }

            result.Version = latestVersion.ToFullString();

            // Retrieve package metadata to get the correct canonical name
            var metadataResource = await repository.GetResourceAsync<PackageMetadataResource>();
            var metadata = await metadataResource.GetMetadataAsync(packageName, true, true, cacheContext, NullLogger.Instance, CancellationToken.None);
            var packageMetadata = metadata.FirstOrDefault();

            if (packageMetadata == null) {
                result.ErrorMessage = $"Could not retrieve package metadata for {packageName}";
                result.Success = false;
                return result;
            }

            string confirmedPackageName = packageMetadata.Identity.Id; // Use the exact name from NuGet

            using var packageStream = new MemoryStream();
            await resource.CopyNupkgToStreamAsync(confirmedPackageName, latestVersion, packageStream, cacheContext, NullLogger.Instance, CancellationToken.None);
            packageStream.Position = 0;

            var packageReader = new PackageArchiveReader(packageStream);

            // Define framework preference order
            string[] preferredFrameworks = { "net8.0", "net7.0", "net6.0", "netstandard2.0", "net5.0" };

            IEnumerable<string> packageFiles = Enumerable.Empty<string>();

            foreach (var framework in preferredFrameworks) {
                packageFiles = packageReader.GetFiles($"lib/{framework}");
                if (packageFiles.Any())
                    break;
            }

            if (!packageFiles.Any()) {
                result.ErrorMessage = $"No compatible assemblies found in {confirmedPackageName}.";
                result.Success = false;
                return result;
            }

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

            result.ConfirmedPackageName = confirmedPackageName;
            result.Path = packagePath;
            result.Success = true;
            NotifyPackagesUpdated();
        }
        catch (Exception ex) {
            result.ErrorMessage = $"Error installing {packageName}: {ex.Message}";
            result.Success = false;
        }

        return result;
    }

    public async Task<PackageInstallResult> UpdatePackageAsync(string packageName) {
        return await InstallPackageAsync(packageName);
    }

    public async Task<PackageUninstallResult> UninstallPackageAsync(string packageName) {
        try {
            var packagePath = Path.Combine(_packageDirectory, packageName);
            if (Directory.Exists(packagePath)) {
                await Task.Run(() => Directory.Delete(packagePath, true));
            }
            else {
                return PackageUninstallResult.NotFound;
            }
        }
        catch (Exception ex) {
            return PackageUninstallResult.Failed;
        }

        NotifyPackagesUpdated();
        return PackageUninstallResult.Success;
    }

    private static string ExtractPackageFile(string sourcePath, string targetPath, Stream stream) {
        try {
            using var fileStream = File.Create(targetPath);
            stream.CopyTo(fileStream);
            return targetPath;
        }
        catch (Exception ex) {
            return string.Empty;
        }
    }

    public IEnumerable<string?> GetInstalledPackages() {
        if (Directory.Exists(_packageDirectory)) {
            var dirs = Directory.GetDirectories(_packageDirectory);
            return dirs.Select(Path.GetFileName).ToList();
        }

        return new List<string?>();
    }

    public async Task<PackageSearchResult> SearchPackagesAsync(string searchTerm) {
        var result = new PackageSearchResult();
        var items = new List<PackageSearchItem>();


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

            foreach (var package in searchResults) {
                items.Add(
                    new PackageSearchItem {
                        Name = package.Identity.Id,
                        Version = package.Identity.Version.ToString(),
                        Description = package.Description
                    }
                );
            }

            result.Success = true;
            result.Items = items;
        }
        catch (Exception ex) {
            result.Success = false;
            result.ErrorMessage = $"Error searching for packages: {ex.Message}";
        }

        return result;
    }

    public IEnumerable<string> GetInstalledPackagePaths() {
        return Directory.EnumerateFiles(_packageDirectory, "*.dll", SearchOption.AllDirectories).ToList();
    }
}
