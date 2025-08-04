using System;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Configuration;

namespace ParksComputing.Xfer.Lang.Converters
{
    /// <summary>
    /// Abstract base class for type-specific XferLang converters.
    /// Provides a strongly-typed foundation for implementing custom conversion logic
    /// for specific .NET types during XferLang serialization and deserialization.
    /// </summary>
    /// <typeparam name="T">The .NET type this converter handles.</typeparam>
    public abstract class XferConverter<T> : IXferConverter
    {
        /// <summary>
        /// Determines if this converter can handle the specified object type.
        /// By default, returns true if the type is assignable from T.
        /// </summary>
        /// <param name="objectType">The type to check for conversion support.</param>
        /// <returns>True if the type can be converted; otherwise, false.</returns>
        public virtual bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        /// <summary>
        /// Converts an object to a XferLang element. Calls the strongly-typed WriteXfer method.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <param name="settings">Serializer settings to control conversion behavior.</param>
        /// <returns>The XferLang element representation.</returns>
        public Element WriteXfer(object value, XferSerializerSettings settings)
        {
            return WriteXfer((T)value, settings);
        }

        /// <summary>
        /// Converts a strongly-typed value to a XferLang element.
        /// Implement this method to provide custom serialization logic for type T.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="settings">Serializer settings to control conversion behavior.</param>
        /// <returns>The XferLang element representation.</returns>
        public abstract Element WriteXfer(T value, XferSerializerSettings settings);

        /// <summary>
        /// Converts a XferLang element to an object. Calls the strongly-typed ReadXfer method.
        /// </summary>
        /// <param name="element">The XferLang element to convert.</param>
        /// <param name="objectType">The target object type.</param>
        /// <param name="settings">Serializer settings to control conversion behavior.</param>
        /// <returns>The deserialized object.</returns>
        public object? ReadXfer(Element element, Type objectType, XferSerializerSettings settings)
        {
            return ReadXfer(element, settings);
        }

        /// <summary>
        /// Converts a XferLang element to a strongly-typed value.
        /// Implement this method to provide custom deserialization logic for type T.
        /// </summary>
        /// <param name="element">The XferLang element to convert.</param>
        /// <param name="settings">Serializer settings to control conversion behavior.</param>
        /// <returns>The deserialized value of type T, or null if conversion fails.</returns>
        public abstract T? ReadXfer(Element element, XferSerializerSettings settings);
    }
}
