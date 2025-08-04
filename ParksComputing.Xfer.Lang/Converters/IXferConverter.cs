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
        bool CanConvert(Type objectType);
        Element WriteXfer(object value, XferSerializerSettings settings);
        object? ReadXfer(Element element, Type objectType, XferSerializerSettings settings);
    }
}
