using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ParksComputing.Xfer.Lang.ContractResolvers {
    /// <summary>
    /// Default implementation of contract resolution for XferLang serialization.
    /// Resolves public instance properties and preserves original property names
    /// without modification. Provides the standard behavior for most serialization scenarios.
    /// </summary>
    public class DefaultContractResolver : IContractResolver {
        public List<PropertyInfo> ResolveProperties(Type type) {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
        }

        public virtual string ResolvePropertyName(string propertyName) {
            return propertyName;
        }
    }
}
