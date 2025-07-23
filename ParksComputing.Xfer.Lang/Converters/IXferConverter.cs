using System;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Configuration;

namespace ParksComputing.Xfer.Lang.Converters
{
    public interface IXferConverter
    {
        bool CanConvert(Type objectType);
        Element WriteXfer(object value, XferSerializerSettings settings);
        object? ReadXfer(Element element, Type objectType, XferSerializerSettings settings);
    }
}
