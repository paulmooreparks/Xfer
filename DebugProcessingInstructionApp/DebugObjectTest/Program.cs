using ParksComputing.Xfer.Lang.Elements;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== ObjectElement Debug ===\n");

        // Test the exact same setup as the failing test
        var obj = new ObjectElement();
        obj["name"] = new StringElement("John");
        obj["age"] = new IntegerElement(30);

        var result = obj.ToXfer();
        Console.WriteLine($"Actual output: '{result}'");

        // Check what the test expects
        Console.WriteLine($"Starts with '{{': {result.StartsWith("{")}");
        Console.WriteLine($"Ends with ' }}': {result.EndsWith(" }")}");
        Console.WriteLine($"Contains 'name \"John\"': {result.Contains("name \"John\"")}");
        Console.WriteLine($"Contains 'age #30': {result.Contains("age #30")}");

        // Show individual pieces
        var kvpName = new KeyValuePairElement(new KeywordElement("name"), new StringElement("John"));
        var kvpAge = new KeyValuePairElement(new KeywordElement("age"), new IntegerElement(30));

        Console.WriteLine($"\nIndividual KeyValuePairs:");
        Console.WriteLine($"kvpName: '{kvpName.ToXfer()}'");
        Console.WriteLine($"kvpAge: '{kvpAge.ToXfer()}'");

        // Show element details
        var intElement = new IntegerElement(30);
        Console.WriteLine($"\nIntegerElement(30): '{intElement.ToXfer()}'");
        Console.WriteLine($"Delimiter style: {intElement.Delimiter.Style}");
        Console.WriteLine($"MinOpening: '{intElement.Delimiter.MinOpening}'");
        Console.WriteLine($"MinClosing: '{intElement.Delimiter.MinClosing}'");

        // Check KeyValuePair element details
        Console.WriteLine($"\nKeyValuePair delimiter details:");
        Console.WriteLine($"kvpAge Delimiter style: {kvpAge.Delimiter.Style}");
        Console.WriteLine($"kvpAge MinOpening: '{kvpAge.Delimiter.MinOpening}'");
        Console.WriteLine($"kvpAge MinClosing: '{kvpAge.Delimiter.MinClosing}'");
        Console.WriteLine($"kvpAge has closing specifier: {!string.IsNullOrEmpty(kvpAge.Delimiter.MinClosing)}");
    }
}
