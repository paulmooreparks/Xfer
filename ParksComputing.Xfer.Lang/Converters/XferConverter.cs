using System;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Configuration;

namespace ParksComputing.Xfer.Lang.Converters
{
    public abstract class XferConverter<T> : IXferConverter
    {
        public virtual bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public Element WriteXfer(object value, XferSerializerSettings settings)
        {
            return WriteXfer((T)value, settings);
        }

        public abstract Element WriteXfer(T value, XferSerializerSettings settings);

        public object? ReadXfer(Element element, Type objectType, XferSerializerSettings settings)
        {
            return ReadXfer(element, settings);
        }

        public abstract T? ReadXfer(Element element, XferSerializerSettings settings);
    }
}
