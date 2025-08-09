using ParksComputing.Xfer.Lang.Elements;

class TestProgram
{
    static void Main()
    {
        var obj = new ObjectElement();
        obj["name"] = new StringElement("John");
        var result = obj.ToXfer();

        Console.WriteLine($"Result: '{result}'");
        Console.WriteLine($"Contains 'name \"John\"': {result.Contains("name \"John\"")}");
        Console.WriteLine($"Starts with '{{': {result.StartsWith("{")}");
        Console.WriteLine($"Ends with '}}': {result.EndsWith("}")}");
    }
}
