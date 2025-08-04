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
        /// <summary>
        /// Resolves all public instance properties of the specified type for serialization.
        /// </summary>
        /// <param name="type">The type to resolve properties for.</param>
        /// <returns>A list of all public instance properties.</returns>
        public List<PropertyInfo> ResolveProperties(Type type) {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
        }

        /// <summary>
        /// Returns the property name unchanged. Override to implement custom name transformation.
        /// </summary>
        /// <param name="propertyName">The original property name.</param>
        /// <returns>The unmodified property name.</returns>
        public virtual string ResolvePropertyName(string propertyName) {
            return propertyName;
        }
    }
}
