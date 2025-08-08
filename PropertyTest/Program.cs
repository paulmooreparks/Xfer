using System;
using System.Linq;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;

namespace PropertyTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Testing manual property assignment ===");

            // Test 1: ID first, then Tag
            var kvp1 = new KeyValuePairElement(new StringElement("name"), new StringElement("Test"));
            Console.WriteLine($"Initial: Id='{kvp1.Id}', Tag='{kvp1.Tag}'");

            kvp1.Id = "user1";
            Console.WriteLine($"After setting Id: Id='{kvp1.Id}', Tag='{kvp1.Tag}'");

            kvp1.Tag = "admin";
            Console.WriteLine($"After setting Tag: Id='{kvp1.Id}', Tag='{kvp1.Tag}'");

            // Test 2: Tag first, then ID
            var kvp2 = new KeyValuePairElement(new StringElement("name"), new StringElement("Test"));
            Console.WriteLine($"\nInitial: Id='{kvp2.Id}', Tag='{kvp2.Tag}'");

            kvp2.Tag = "admin";
            Console.WriteLine($"After setting Tag: Id='{kvp2.Id}', Tag='{kvp2.Tag}'");

            kvp2.Id = "user1";
            Console.WriteLine($"After setting Id: Id='{kvp2.Id}', Tag='{kvp2.Tag}'");

            // Test 3: Check if the Tag setter is somehow affecting the Id
            var kvp3 = new KeyValuePairElement(new StringElement("name"), new StringElement("Test"));
            kvp3.Id = "user1";
            Console.WriteLine($"\nBefore Tag assignment: Id='{kvp3.Id}', Tag='{kvp3.Tag}'");

            // Multiple tag assignments to see if there's side effects
            kvp3.Tag = "temp";
            Console.WriteLine($"After temp tag: Id='{kvp3.Id}', Tag='{kvp3.Tag}'");

            kvp3.Tag = "admin";
            Console.WriteLine($"After final tag: Id='{kvp3.Id}', Tag='{kvp3.Tag}'");

            Console.WriteLine("\n=== Check property get/set behavior ===");
            var kvp4 = new KeyValuePairElement(new StringElement("name"), new StringElement("Test"));

            // Direct property access
            kvp4.Id = "direct-id";
            kvp4.Tag = "direct-tag";
            Console.WriteLine($"Direct assignment: Id='{kvp4.Id}', Tag='{kvp4.Tag}'");

            // Check if properties are working correctly
            string? retrievedId = kvp4.Id;
            string? retrievedTag = kvp4.Tag;
            Console.WriteLine($"Retrieved: Id='{retrievedId}', Tag='{retrievedTag}'");
        }
    }
}
