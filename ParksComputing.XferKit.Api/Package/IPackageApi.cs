using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.XferKit.Api.Package;

public interface IPackageApi {
    PackageApiResult install(string packageName);
    Task<PackageApiResult> installAsync(string packageName);
    PackageApiResult uninstall(string packageName);
    Task<PackageApiResult> uninstallAsync(string packageName);
    PackageApiResult update(string packageName);
    Task<PackageApiResult> updateAsync(string packageName);
    PackageApiResult search(string search);
    Task<PackageApiResult> searchAsync(string search);
    string[] list { get; }
}

public class PackageApiResult {
    public bool success { get; set; }
    public string? message { get; set; }
    public string? packageName { get; set; }
    public string? version { get; set; }
    public string? path { get; set; }
    public string[]? list { get; set; }
}
