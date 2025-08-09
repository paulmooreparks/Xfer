using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.ProcessingInstructions;

class Program
{
    static void Main()
    {
        // Test different ObjectElement scenarios

        // Test with string value (compact syntax)
        var obj1 = new ObjectElement();
        obj1["name"] = new StringElement("John");
        Console.WriteLine($"With string: '{obj1.ToXfer()}'");

        // Test with integer value (implicit syntax)
        var obj2 = new ObjectElement();
        obj2["age"] = new IntegerElement(25);
        Console.WriteLine($"With integer: '{obj2.ToXfer()}'");

        // Test with both
        var obj3 = new ObjectElement();
        obj3["name"] = new StringElement("John");
        obj3["age"] = new IntegerElement(25);
        Console.WriteLine($"With both: '{obj3.ToXfer()}'");

        // Test individual KVPs
        var kvpString = new KeyValuePairElement(new KeywordElement("name"), new StringElement("John"));
        var kvpInt = new KeyValuePairElement(new KeywordElement("age"), new IntegerElement(25));

        Console.WriteLine($"KVP string: '{kvpString.ToXfer()}'");
        Console.WriteLine($"KVP int: '{kvpInt.ToXfer()}'");

        // Test the elements individually
        var str = new StringElement("John");
        var num = new IntegerElement(25);
        Console.WriteLine($"StringElement: '{str.ToXfer()}'");
        Console.WriteLine($"IntegerElement: '{num.ToXfer()}'");
        Console.WriteLine($"String delimiter style: {str.Delimiter.Style}");
        Console.WriteLine($"Integer delimiter style: {num.Delimiter.Style}");
    }
}
