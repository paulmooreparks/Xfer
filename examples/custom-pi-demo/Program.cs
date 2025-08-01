using System;
using System.IO;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.DynamicSource;

namespace CustomPiDemo {
    class Program {
        static void Main() {
            Console.WriteLine("Custom PI Demo");
        }
    #if false
        static void Main(string[] args) {
            // Example XferLang document with metadata PI and a custom PI
            string xfer =
                "<! id \"foo\" !>\n" +
                "person {\n" +
                "  name \"Alice\"\n" +
                "  age 30\n" +
                "}\n" +
                "<?log \"Hello, world!\"?>\n";

            // Parse the document (assuming a Parse method exists)
            XferDocument doc = XferParser.Parse(xfer);

            // Find element by ID (demo logic)
            var element = FindElementById(doc, "foo");
            if (element != null) {
                Console.WriteLine("Found element with id=foo:");
                Console.WriteLine(element.ToString());
            }
            else {
                Console.WriteLine("Element with id=foo not found.");
            }

            // Process custom PI (e.g., log PI)
            ProcessCustomPis(doc);
        }

        // Demo: Find an element by its metadata id
        static Element? FindElementById(XferDocument doc, string id) {
            // In a real implementation, traverse the document tree and check metadata for id
            // Here, just simulate finding the root if the id matches
            if (doc.Root is TupleElement tuple) {
                foreach (var elem in tuple.Values) {
                    if (elem is MetadataElement meta && meta.ContainsKey("id") && meta["id"] is StringElement se && se.Value == id) {
                        // The next element after metadata is the target
                        int idx = tuple.Values.IndexOf(meta);
                        if (idx + 1 < tuple.Values.Count()) {
                            return tuple.Values[idx + 1];
                        }
                    }
                }
            }
            return null;
        }

        // Demo: Process custom processing instructions (PIs)
        static void ProcessCustomPis(XferDocument doc) {
            // In a real implementation, scan for MetadataElement or PIElement with custom keys
            // Here, just simulate finding a log PI
            if (doc.Root is TupleElement tuple) {
                foreach (var elem in tuple.Values) {
                    if (elem is ProcessingInstructionElement pi && pi.ContainsKey("log")) {
                        var logValue = pi["log"];
                        Console.WriteLine($"[LOG PI] {logValue}");
                    }
                }
            }
        }
    }

    // Example: Custom resolver supporting a 'custom' keyword
    public class CustomDynamicSourceResolver : DefaultDynamicSourceResolver {
        // This assumes you add a protected virtual method in DefaultDynamicSourceResolver for keyword handling
        protected virtual string? ResolveKeyword(string keyword, string value) {
            if (string.Equals(keyword, "custom", StringComparison.OrdinalIgnoreCase)) {
                return $"CUSTOM({value})";
            }
            // Fallback to base behavior for known keywords
            if (string.Equals(keyword, "file", StringComparison.OrdinalIgnoreCase)) {
                if (File.Exists(value)) return File.ReadAllText(value);
            } else if (string.Equals(keyword, "env", StringComparison.OrdinalIgnoreCase)) {
                return Environment.GetEnvironmentVariable(value);
            } else if (string.Equals(keyword, "const", StringComparison.OrdinalIgnoreCase)) {
                return value;
            }
            return null;
        }
#endif
    }

    // (No stub needed; use the real XferParser from ParksComputing.Xfer.Lang)
}
