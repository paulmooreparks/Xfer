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
        /// <summary>
        /// Resolves which properties of a type should be included in serialization.
        /// Returns a list of PropertyInfo objects representing the properties to serialize.
        /// </summary>
        /// <param name="type">The type to resolve properties for.</param>
        /// <returns>A list of properties to include in serialization.</returns>
        List<PropertyInfo> ResolveProperties(Type type);

        /// <summary>
        /// Resolves the name to use for a property during serialization.
        /// Allows transformation of property names (e.g., camelCase conversion).
        /// </summary>
        /// <param name="propertyName">The original property name.</param>
        /// <returns>The transformed property name to use in the serialized output.</returns>
        string ResolvePropertyName(string propertyName);
    }
}
