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
            var doc = parser.Parse(input);

            var xfer = doc.ToXfer(Formatting.Pretty);
            Console.WriteLine("Serialized Xfer:");
            Console.WriteLine(xfer);

            var root = doc.GetElementById("root");
            if (root != null) {
                Console.WriteLine($"Root Element: {root.ToXfer()}");
            }
            else {
                Console.WriteLine("Root element not found.");
            }

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

            // Test finding element by ID
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
                var meta = element.Metadata;
                if (meta != null && (!string.IsNullOrEmpty(meta.Xfer) || !string.IsNullOrEmpty(meta.Version) || meta.Extensions.Count > 0)) {
                    Console.WriteLine($"  {(label ?? element.GetType().Name)}:");
                    if (!string.IsNullOrEmpty(meta.Xfer)) {
                        Console.WriteLine($"    Version: {meta.Xfer}");
                    }
                    if (!string.IsNullOrEmpty(meta.Version)) {
                        Console.WriteLine($"    DocumentVersion: {meta.Version}");
                    }
                    foreach (var ext in meta.Extensions) {
                        if (ext.Value is Dictionary<string, int> dict) {
                            Console.WriteLine($"    {ext.Key}:");
                            foreach (var cdef in dict) {
                                Console.WriteLine($"      {cdef.Key} = U+{cdef.Value:X4}");
                            }
                        }
                        else {
                            Console.WriteLine($"    {ext.Key}: {ext.Value}");
                        }
                    }
                }
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
            // Print only the 'alphabet' object and its children for clarity
            // doc.Root is DocumentElement; search its children for the 'alphabet' KeyValuePairElement
            var alphabetKvp = doc.Root.Children
                .OfType<KeyValuePairElement>()
                .FirstOrDefault(kvp => kvp.Key == "alphabet");
            if (alphabetKvp != null) {
                Console.WriteLine("  alphabet:");
                PrintElementMetadata(alphabetKvp.Value, "alphabet");
            }
            else {
                PrintElementMetadata(doc.Root);
            }

            Console.WriteLine("\nResolved characters:");
            void PrintCharacters(Element element, string? label = null) {
                if (element is CharacterElement charElem) {
                    Console.WriteLine($"  {(label ?? element.GetType().Name)}: {char.ConvertFromUtf32(charElem.Value)} (U+{charElem.Value:X4})");
                }
                // Recurse into children if any
                if (element is ObjectElement obj) {
                    foreach (var kv in obj.Dictionary) {
                        PrintCharacters(kv.Value, kv.Key);
                    }
                }
                else if (element is ArrayElement arr) {
                    foreach (var item in arr.Values) {
                        PrintCharacters(item);
                    }
                }
                else if (element is KeyValuePairElement kvp) {
                    // Only print the value, but do not recurse further (avoid double recursion)
                    PrintCharacters(kvp.Value, kvp.Key);
                }
            }
            // Print only the 'alphabet' object and its children for clarity
            var alphabetKvp2 = doc.Root.Children
                .OfType<KeyValuePairElement>()
                .FirstOrDefault(kvp => kvp.Key == "alphabet");
            if (alphabetKvp2 != null) {
                Console.WriteLine("  alphabet:");
                PrintCharacters(alphabetKvp2.Value, "alphabet");
            }
            else {
                PrintCharacters(doc.Root);
            }
        }
    }
}
