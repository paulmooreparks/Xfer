using System.Linq;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.Helpers {
    /// <summary>
    /// Utility to deep-clone an element via serialize/parse round-trip. Trades performance for simplicity.
    /// Optimizable later with explicit clone graph.
    /// </summary>
    internal static class ElementCloner {
        public static Element Clone(Element element) {
            if (element == null) {
                return new EmptyElement();
            }
            // Wrap in tuple to ensure a single root collection for parsing
            var tmp = $"({element.ToXfer()})";
            var parser = new Parser();
            var doc = parser.Parse(tmp);
            var tuple = doc.Root as TupleElement;
            if (tuple != null && tuple.Children.Count > 0) {
                return tuple.Children[0];
            }
            return new EmptyElement();
        }
    }
}
