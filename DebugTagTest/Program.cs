using System;
using System.Linq;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;

namespace DebugTagTest
{
    class Program
    {
        static void PrintElementTree(Element element, string indent = "")
        {
            Console.WriteLine($"{indent}{element.GetType().Name} - Id: '{element.Id ?? "null"}', Tag: '{element.Tag ?? "null"}'");
            foreach (var child in element.Children)
            {
                PrintElementTree(child, indent + "  ");
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("=== Correct syntax: PIs as siblings (same level as KVP) ===");
            var xferContent1 = """
            {
                <!id "user1"!>
                <!tag "admin"!>
                name "Admin User"
            }
            """;

            var document1 = XferParser.Parse(xferContent1);
            PrintElementTree(document1.Root);

            var kvp1 = document1.Root.Children.OfType<KeyValuePairElement>().FirstOrDefault();
            Console.WriteLine($"KVP1: Id='{kvp1?.Id}', Tag='{kvp1?.Tag}'");
            Console.WriteLine($"GetElementById: {document1.GetElementById("user1")?.GetType().Name ?? "null"}");

            Console.WriteLine("\n=== Test duplicate tags (should throw error) ===");
            var xferContent2 = """
            {
                <!tag "first"!>
                <!tag "second"!>
                name "This should fail"
            }
            """;

            try
            {
                var document2 = XferParser.Parse(xferContent2);
                Console.WriteLine("ERROR: Should have thrown exception!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Expected error: {ex.Message}");
            }
        }
    }
}
