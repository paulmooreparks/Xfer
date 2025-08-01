using System;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Services;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.DynamicSource;
using ParksComputing.Xfer.Lang.ProcessingInstructions;

namespace DynamicResolverDemo
{
    // Custom resolver: supports "reverse:" source type, expects key/value pairs directly in dynamicSource
    public class ReverseDynamicSourceResolver : DefaultDynamicSourceResolver
    {
        public override string? Resolve(string key, XferDocument document)
        {
            foreach (var meta in document.Root.Values)
            {
                if (meta is ProcessingInstruction metaElem && metaElem.Kvp?.Key == "dynamicSource")
                {
                    if (metaElem.Kvp.Value is ObjectElement obj && obj.ContainsKey(key))
                    {
                        Element? currentElem = obj[key];
                        while (currentElem is KeyValuePairElement kvElem2) {
                            currentElem = kvElem2.Value;
                        }
                        string? sourceStr = null;
                        if (currentElem is StringElement strElem) {
                            sourceStr = strElem.Value;
                        } else {
                            sourceStr = currentElem?.ToString();
                        }
                        if (sourceStr != null && sourceStr.StartsWith("reverse:")) {
                            var text = sourceStr.Substring(8);
                            char[] arr = text.ToCharArray();
                            Array.Reverse(arr);
                            return new string(arr);
                        }
                    }
                }
            }
            // Fallback to base resolver
            return base.Resolve(key, document);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Read Xfer document from file
            var xferPath = "sample.xfer";
            var xfer = System.IO.File.ReadAllText(xferPath);

            var parser = new Parser();
            parser.DynamicSourceResolver = new ReverseDynamicSourceResolver();
            var doc = parser.Parse(xfer);
            var root = doc.Root;
            foreach (var element in root.Values)
            {
                if (element is KeyValuePairElement kvp && kvp.Key == "message")
                {
                    var obj = kvp.Value as ObjectElement;
                    if (obj != null)
                    {
                        var textRaw = obj["text"];
                        var greetRaw = obj["greeting"];
                        Element? textValue = textRaw is KeyValuePairElement textKvp ? textKvp.Value : textRaw;
                        Element? greetValue = greetRaw is KeyValuePairElement greetKvp ? greetKvp.Value : greetRaw;

                        if (textValue is InterpolatedElement textElement) {
                            Console.WriteLine($"Resolved text: {textElement.Value}");
                        } else if (textValue is StringElement strElem) {
                            Console.WriteLine($"Resolved text (string): {strElem.Value}");
                        } else {
                            Console.WriteLine($"Resolved text (raw): {textValue}");
                        }

                        if (greetValue is InterpolatedElement greetElement) {
                            Console.WriteLine($"Resolved greeting: {greetElement.Value}");
                        } else if (greetValue is StringElement greetStrElem) {
                            Console.WriteLine($"Resolved greeting (string): {greetStrElem.Value}");
                        } else {
                            Console.WriteLine($"Resolved greeting (raw): {greetValue}");
                        }
                    }
                }
            }
        }
    }
}
