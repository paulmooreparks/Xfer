using ParksComputing.Xfer.Lang.Elements;

var obj = new ObjectElement();
obj["name"] = new StringElement("John");
obj["age"] = new IntegerElement(30);

var result = obj.ToXfer();
Console.WriteLine($"Actual output: '{result}'");
Console.WriteLine($"Length: {result.Length}");
Console.WriteLine("Starts with '{{ ': " + result.StartsWith("{ "));
Console.WriteLine("Ends with ' }}': " + result.EndsWith(" }"));
Console.WriteLine("Contains 'name \"John\"': " + result.Contains("name \"John\""));
Console.WriteLine("Contains 'age #30': " + result.Contains("age #30"));
