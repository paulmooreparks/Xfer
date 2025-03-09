using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.XferKit.Api.Package;

public interface IPackageApi {
    bool install(string packageName);
    bool uninstall(string packageName);
    bool update(string packageName);
    string[] search(string search);
    string[] list { get; }
}
