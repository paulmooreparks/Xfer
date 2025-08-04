using System;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Configuration;

namespace ParksComputing.Xfer.Lang.Converters
{
    /// <summary>
    /// Defines custom conversion logic for XferLang serialization and deserialization.
    /// Custom converters enable specialized handling of specific types during the
    /// conversion process between .NET objects and XferLang elements.
    /// </summary>
    public interface IXferConverter
    {
        /// <summary>
        /// Determines whether this converter can handle the specified object type.
        /// </summary>
        /// <param name="objectType">The type to check for conversion support.</param>
        /// <returns>True if this converter can handle the type; otherwise, false.</returns>
        bool CanConvert(Type objectType);

        /// <summary>
        /// Converts a .NET object to a XferLang element during serialization.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <param name="settings">Serializer settings to control conversion behavior.</param>
        /// <returns>The XferLang element representation of the object.</returns>
        Element WriteXfer(object value, XferSerializerSettings settings);

        /// <summary>
        /// Converts a XferLang element back to a .NET object during deserialization.
        /// </summary>
        /// <param name="element">The XferLang element to convert.</param>
        /// <param name="objectType">The target .NET type to convert to.</param>
        /// <param name="settings">Serializer settings to control conversion behavior.</param>
        /// <returns>The deserialized .NET object, or null if conversion fails.</returns>
        object? ReadXfer(Element element, Type objectType, XferSerializerSettings settings);
    }
}
