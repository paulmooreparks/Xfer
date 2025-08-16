using System.Collections.Generic;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang {
    /// <summary>
    /// Represents a chained keyword/value path with a terminal payload element.
    /// For example, the Xfer value <c>javascript <"scriptBody()"></c> maps to Keys=["javascript"], Payload=StringElement("scriptBody()").
    /// When no chained keywords are present (e.g., just <c><"scriptBody()"></c>) then Keys is empty.
    /// </summary>
    public sealed class XferKeyedValue {
        /// <summary>The sequence of nested keyword keys leading to the payload.</summary>
        public IReadOnlyList<string> Keys { get; }

        /// <summary>The terminal payload element (e.g., string, text, tuple, etc.).</summary>
        public Element Payload { get; }

        /// <summary>
        /// Convenience accessor: returns the payload as a string if it is a string-like element; otherwise null.
        /// </summary>
        public string? PayloadAsString {
            get {
                return Payload switch {
                    StringElement s => s.Value,
                    TextElement t => t.Value,
                    _ => null
                };
            }
        }

        public XferKeyedValue(IReadOnlyList<string> keys, Element payload) {
            Keys = keys;
            Payload = payload;
        }
    }
}
