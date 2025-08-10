using System;
using System.IO;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcessingInstructionDemo {
    class Program {
        static void Main(string[] args) {
            Console.OutputEncoding = Encoding.UTF8;

            var filePath = args.Length > 0 ? args[0] : "sample.xfer";
            if (!File.Exists(filePath)) {
                Console.WriteLine($"File not found: {filePath}");
                return;
            }

            var parser = new Parser();
            // Register CharDefProcessor for extensible charDef handling
            var charDefProcessor = CharDefProcessorSetup.RegisterWith(parser);

            var input = File.ReadAllText(filePath);
            XferDocument? doc = null;
            try {
                doc = parser.Parse(input);
            }
            catch (Exception ex) {
                Console.WriteLine("Parse error:");
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null) {
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");
                }
                return; // Cannot continue without a document
            }

            // Report warnings (e.g., unknown conditional operators)
            if (doc.Warnings.Any()) {
                Console.WriteLine("\nParse Warnings:");
                foreach (var w in doc.Warnings) {
                    Console.WriteLine($"  [{w.Type}] {w.Message} (row {w.Row}, col {w.Column})");
                }
            }
            else {
                Console.WriteLine("\nNo parse warnings.");
            }

            // Placeholder for future error collection if added to XferDocument
            // if (doc.Errors?.Any() == true) { ... }


            var xfer = doc.ToXfer();
            Console.WriteLine("Serialized Xfer:");
            Console.WriteLine(xfer);

            var kvp = doc.GetElementById("kvp");
            if (kvp != null) {
                Console.WriteLine($"KVP Element: {kvp.ToXfer()}");
            }
            else {
                Console.WriteLine("KVP element not found.");
            }

            var value = doc.GetElementById("value");
            if (value != null) {
                Console.WriteLine($"Value Element: {value.ToXfer()}");
            }
            else {
                Console.WriteLine("Value element not found.");
            }

            var objectElement = doc.GetElementById("object");
            if (objectElement != null) {
                Console.WriteLine($"Object Element: {objectElement.ToXfer()}");
            }
            else {
                Console.WriteLine("Object element not found.");
            }

            var testpiElement = doc.GetElementById("testpi");
            if (testpiElement != null) {
                Console.WriteLine(testpiElement);
            }
            else {
                Console.WriteLine("Element with ID 'testpi' not found.");
            }

            var array = doc.GetElementById("array");
            if (array != null) {
                Console.WriteLine($"Array Element: {array.ToXfer()}");
            }
            else {
                Console.WriteLine("Element with ID 'array' not found.");
            }

            var one = doc.GetElementById("one");
            if (one != null) {
                Console.WriteLine($"Element with ID 'one': {one.ToXfer()}");
            }
            else {
                Console.WriteLine("Element with ID 'one' not found.");
            }

            Console.WriteLine("Document-level metadata:");

            if (doc.Metadata != null) {
                Console.WriteLine($"  Version: {doc.Metadata.Xfer}");
                Console.WriteLine($"  DocumentVersion: {doc.Metadata.Version}");
                foreach (var ext in doc.Metadata.Extensions) {
                    if (ext.Value is Dictionary<string, int> dict) {
                        Console.WriteLine($"  {ext.Key}:");
                        foreach (var kv in dict) {
                            Console.WriteLine($"    {kv.Key}: U+{kv.Value:X4}");
                        }
                    }
                    else {
                        Console.WriteLine($"  {ext.Key}: {ext.Value}");
                    }
                }
            }
            else {
                Console.WriteLine("  (none)");
            }

            Console.WriteLine("\nElement-level metadata:");
            void PrintElementMetadata(Element element, string? label = null) {
                // Recurse into children if any
                if (element is ObjectElement obj) {
                    foreach (var kv in obj.Dictionary) {
                        PrintElementMetadata(kv.Value, kv.Key);
                    }
                }
                else if (element is ArrayElement arr) {
                    foreach (var item in arr.Values) {
                        PrintElementMetadata(item);
                    }
                }
                else if (element is KeyValuePairElement kvp) {
                    // Only print the value, but do not recurse further (avoid double recursion)
                    PrintElementMetadata(kvp.Value, kvp.Key);
                }
            }
        }
    }
}
