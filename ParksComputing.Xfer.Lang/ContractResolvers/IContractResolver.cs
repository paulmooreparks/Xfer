using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ParksComputing.Xfer.Lang.ContractResolvers {
    public interface IContractResolver {
        List<PropertyInfo> ResolveProperties(Type type);
        string ResolvePropertyName(string propertyName);
    }
}
