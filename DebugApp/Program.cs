using ParksComputing.Xfer.Lang.Elements;

var obj = new ObjectElement();
obj["name"] = new StringElement("John");
obj["age"] = new IntegerElement(30);

var result = obj.ToXfer();
Console.WriteLine($"Actual output: '{result}'");
Console.WriteLine($"Length: {result.Length}");

// Debug the key-value pair structure
foreach (var child in obj.Children)
{
    if (child is KeyValuePairElement kvp)
    {
        Console.WriteLine($"Key type: {kvp.KeyElement.GetType().Name}");
        Console.WriteLine($"Value type: {kvp.Value.GetType().Name}");
        Console.WriteLine($"Value delimiter style: {kvp.Value.Delimiter.Style}");
        Console.WriteLine($"KVP ToXfer: '{kvp.ToXfer()}'");
    }
}
