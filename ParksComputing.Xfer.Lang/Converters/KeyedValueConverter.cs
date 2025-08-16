using System;
using System.Collections.Generic;
using ParksComputing.Xfer.Lang.Configuration;
using ParksComputing.Xfer.Lang.Converters;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Converters {
    /// <summary>
    /// Converter for XferKeyedValue. Reads nested KeyValuePairElement chains into a flattened key list + terminal payload element.
    /// Writes an XferKeyedValue by wrapping the payload element in nested key/value pairs according to Keys.
    /// </summary>
    public sealed class KeyedValueConverter : XferConverter<XferKeyedValue> {
        public override bool CanConvert(Type objectType) => typeof(XferKeyedValue).IsAssignableFrom(objectType);

        public override Element WriteXfer(XferKeyedValue value, XferSerializerSettings settings) {
            Element current = value.Payload;
            // Wrap keys from last to first
            for (int i = value.Keys.Count - 1; i >= 0; i--) {
                current = new KeyValuePairElement(new KeywordElement(value.Keys[i]), current);
            }
            return current;
        }

        public override XferKeyedValue? ReadXfer(Element element, XferSerializerSettings settings) {
            var keys = new List<string>();
            Element current = element;
            while (current is KeyValuePairElement kvp) {
                keys.Add(kvp.Key);
                current = kvp.Value;
            }
            return new XferKeyedValue(keys, current);
        }
    }
}
