using System;
using ParksComputing.Xfer.Lang.Elements;

namespace DebugObjectSpacing
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Object Element Spacing Debug ===");

            // Test single key-value pair
            var obj1 = new ObjectElement();
            obj1["name"] = new StringElement("John");
            string result1 = obj1.ToXfer();
            Console.WriteLine($"Single key-value: '{result1}'");
            Console.WriteLine($"Contains 'name \"John\"': {result1.Contains("name \"John\"")}");

            // Test multiple key-value pairs
            var obj2 = new ObjectElement();
            obj2["name"] = new StringElement("John");
            obj2["age"] = new IntegerElement(25);
            string result2 = obj2.ToXfer();
            Console.WriteLine($"Multiple key-values: '{result2}'");

            // Test key with integer requiring space
            var obj3 = new ObjectElement();
            obj3["count"] = new IntegerElement(42);
            string result3 = obj3.ToXfer();
            Console.WriteLine($"Key with integer: '{result3}'");
            Console.WriteLine($"Contains 'count 42': {result3.Contains("count 42")}");

            // Test multiple integers
            var obj4 = new ObjectElement();
            obj4["num1"] = new IntegerElement(1);
            obj4["num2"] = new IntegerElement(2);
            string result4 = obj4.ToXfer();
            Console.WriteLine($"Multiple integers: '{result4}'");

            Console.WriteLine("=== End Debug ===");
        }
    }
}
