using ParksComputing.Xfer.Lang.ContractResolvers;
using ParksComputing.Xfer.Lang.Converters;
using System.Collections.Generic;

namespace ParksComputing.Xfer.Lang.Configuration {
    /// <summary>
    /// Configuration settings for XferLang serialization and deserialization.
    /// Controls how .NET objects are converted to/from XferLang format, including
    /// null handling, contract resolution, custom converters, and element styling preferences.
    /// </summary>
    public class XferSerializerSettings {
        /// <summary>
        /// Specifies how null values are handled during serialization.
        /// Controls whether null properties are included or excluded from the output.
        /// </summary>
        public NullValueHandling NullValueHandling { get; set; } = NullValueHandling.Include;

        /// <summary>
        /// The contract resolver used to determine which properties to serialize
        /// and how to resolve property names during serialization.
        /// </summary>
        public IContractResolver ContractResolver { get; set; } = new DefaultContractResolver();

        /// <summary>
        /// Collection of custom converters for handling specific types during serialization.
        /// Converters are checked in order and the first matching converter is used.
        /// </summary>
        public IList<IXferConverter> Converters { get; } = [];

        /// <summary>
        /// Controls how elements are serialized for compactness vs safety.
        /// Default is CompactWhenSafe for optimal balance of safety and readability.
        /// </summary>
        public ElementStylePreference StylePreference { get; set; } = ElementStylePreference.CompactWhenSafe;

        /// <summary>
        /// When StylePreference allows it, prefer implicit syntax for simple values.
        /// For example, serialize integers as "42" instead of "#42".
        /// </summary>
        public bool PreferImplicitSyntax { get; set; } = false;

        /// <summary>
        /// When true, DateTime values preserve original precision instead of adding microseconds.
        /// Helps maintain round-trip consistency with original documents.
        /// </summary>
        public bool PreserveDateTimePrecision { get; set; } = false;
    }
}
