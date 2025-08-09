using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang;

class Program
{
    static void Main()
    {
        // Test different delimiter styles to verify spacing logic

        // Test with string (compact delimiter - should be no space)
        var kvpString = new KeyValuePairElement(new KeywordElement("name"), new StringElement("John"));
        Console.WriteLine($"String (compact): '{kvpString.ToXfer()}' - Expected: name\"John\"");

        // Test with integer using compact delimiter (should be no space)
        var intElement = new IntegerElement(25);
        intElement.Delimiter = new ElementDelimiter('#', ' ', 1, ElementStyle.Compact);
        var kvpIntCompact = new KeyValuePairElement(new KeywordElement("age"), intElement);
        Console.WriteLine($"Integer (compact): '{kvpIntCompact.ToXfer()}' - Expected: age#25");

        // Test with integer using implicit delimiter (should have space)
        var intElementImplicit = new IntegerElement(42);
        intElementImplicit.Delimiter = new ElementDelimiter('\0', '\0', 1, ElementStyle.Implicit);
        var kvpIntImplicit = new KeyValuePairElement(new KeywordElement("count"), intElementImplicit);
        Console.WriteLine($"Integer (implicit): '{kvpIntImplicit.ToXfer()}' - Expected: count 42");

        // Test with boolean using implicit delimiter (should have space)
        var boolElement = new BooleanElement(true);
        boolElement.Delimiter = new ElementDelimiter('\0', '\0', 1, ElementStyle.Implicit);
        var kvpBool = new KeyValuePairElement(new KeywordElement("active"), boolElement);
        Console.WriteLine($"Boolean (implicit): '{kvpBool.ToXfer()}' - Expected: active true");

        // Test ObjectElement with mixed values
        var obj = new ObjectElement();
        obj["name"] = new StringElement("John");  // Should be name"John" (no space)

        var ageElement = new IntegerElement(25);
        ageElement.Delimiter = new ElementDelimiter('\0', '\0', 1, ElementStyle.Implicit);
        obj["age"] = ageElement;  // Should be age 25 (with space)

        Console.WriteLine($"Mixed object: '{obj.ToXfer()}'");
        Console.WriteLine($"Expected: {{name\"John\" age 25}}");

        // Show delimiter styles
        Console.WriteLine($"StringElement delimiter: {new StringElement("test").Delimiter.Style}");
        Console.WriteLine($"IntegerElement delimiter: {new IntegerElement(123).Delimiter.Style}");
        Console.WriteLine($"BooleanElement delimiter: {new BooleanElement(true).Delimiter.Style}");
    }
}
