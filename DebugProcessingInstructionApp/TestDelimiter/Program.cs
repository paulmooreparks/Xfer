using ParksComputing.Xfer.Lang.Elements;

class TestProgram
{
    static void Main()
    {
        Console.WriteLine("=== SPACING BEHAVIOR TEST ===\n");

        // Test 1: Individual elements (should have no trailing spaces)
        Console.WriteLine("1. Individual elements (no trailing spaces):");
        var intElement = new IntegerElement(25);
        var longElement = new LongElement(123L);
        var doubleElement = new DoubleElement(3.14);
        var stringElement = new StringElement("value");
        var keyword = new KeywordElement("name");

        Console.WriteLine($"   IntegerElement: '{intElement.ToXfer()}'");
        Console.WriteLine($"   LongElement: '{longElement.ToXfer()}'");
        Console.WriteLine($"   DoubleElement: '{doubleElement.ToXfer()}'");
        Console.WriteLine($"   StringElement: '{stringElement.ToXfer()}'");
        Console.WriteLine($"   KeywordElement: '{keyword.ToXfer()}'");

        // Test 2: KeyValuePair spacing based on value delimiters
        Console.WriteLine("\n2. KeyValuePair internal spacing:");
        var kvpWithString = new KeyValuePairElement(new KeywordElement("name"), new StringElement("value"));
        var kvpWithInt = new KeyValuePairElement(new KeywordElement("number"), new IntegerElement(42));
        var kvpWithKeyword = new KeyValuePairElement(new KeywordElement("status"), new KeywordElement("active"));

        Console.WriteLine($"   name + string:   '{kvpWithString.ToXfer()}'  (no space - string has closing delimiter)");
        Console.WriteLine($"   number + int:    '{kvpWithInt.ToXfer()}'     (no space - int has closing delimiter)");
        Console.WriteLine($"   status + keyword: '{kvpWithKeyword.ToXfer()}'  (space - keyword is implicit, no delimiter)");

        // Test 3: Object collection spacing
        Console.WriteLine("\n3. Object collection spacing:");
        var obj1 = new ObjectElement();
        obj1.Add(new KeyValuePairElement(new KeywordElement("name"), new StringElement("test")));
        obj1.Add(new KeyValuePairElement(new KeywordElement("value"), new IntegerElement(42)));

        var obj2 = new ObjectElement();
        obj2.Add(new KeyValuePairElement(new KeywordElement("price"), new DoubleElement(9.99)));
        obj2.Add(new KeyValuePairElement(new KeywordElement("currency"), new StringElement("USD")));
        obj2.Add(new KeyValuePairElement(new KeywordElement("active"), new KeywordElement("true")));

        Console.WriteLine($"   Two pairs:   '{obj1.ToXfer()}'");
        Console.WriteLine($"   Three pairs: '{obj2.ToXfer()}'");

        Console.WriteLine("\n=== TEST COMPLETE ===");
    }
}
