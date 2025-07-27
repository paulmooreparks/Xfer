using System;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Deserialization;

namespace xferc {
    public class DemoDeserializationPI {
        public static void Run() {
            // Example Xfer document with global and inline deserialization PI
            var doc = new XferDocument();
            var globalPI = new ProcessingInstructionElement(ProcessingInstructionElement.DeserializeKeyword);
            doc.MetadataCollection.Add(globalPI);

            var inlinePI = new ProcessingInstructionElement(ProcessingInstructionElement.DeserializeKeyword);
            var key = new IdentifierElement(ProcessingInstructionElement.DeserializeKeyword);
            var kvp = new KeyValuePairElement(key, inlinePI);
            var element = new MetadataElement();
            element.Add(kvp);
            doc.Root.Add(element);

            var resolver = new DefaultDeserializationInstructionResolver();
            var resolved = resolver.ResolveInstructions(element, doc);

            Console.WriteLine("Resolved PI type: " + ((resolved as ProcessingInstructionElement)?.PIType ?? "null"));
            // Should print 'deserialize' (inline PI takes precedence)
        }
    }
}
