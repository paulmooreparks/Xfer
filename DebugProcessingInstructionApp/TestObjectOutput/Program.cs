using ParksComputing.Xfer.Lang.Elements;

class Program
{
    static void Main()
    {
        // Test single key-value
        var obj1 = new ObjectElement();
        obj1["name"] = new StringElement("John");
        Console.WriteLine($"Single: '{obj1.ToXfer()}'");

        // Test multiple key-values
        var obj2 = new ObjectElement();
        obj2["name"] = new StringElement("John");
        obj2["age"] = new IntegerElement(30);
        Console.WriteLine($"Multiple: '{obj2.ToXfer()}'");

        // Test individual KVP
        var kvp = new KeyValuePairElement(new KeywordElement("name"), new StringElement("John"));
        Console.WriteLine($"KVP: '{kvp.ToXfer()}'");
    }
}
