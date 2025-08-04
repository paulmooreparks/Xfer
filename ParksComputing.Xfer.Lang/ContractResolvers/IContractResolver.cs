using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ParksComputing.Xfer.Lang.ContractResolvers {
    /// <summary>
    /// Defines contract resolution for XferLang serialization. Contract resolvers determine
    /// which properties to serialize and how property names should be transformed during
    /// the serialization process.
    /// </summary>
    public interface IContractResolver {
        List<PropertyInfo> ResolveProperties(Type type);
        string ResolvePropertyName(string propertyName);
    }
}
