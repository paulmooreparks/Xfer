using ParksComputing.Xfer.Lang.Elements;

var obj = new ObjectElement();
obj.Delimiter.Style = ElementStyle.Explicit;
obj["name"] = new StringElement("John");

var result = obj.ToXfer();
Console.WriteLine($"Explicit output: '{result}'");
Console.WriteLine("Starts with '<{{ ': " + result.StartsWith("<{ "));
Console.WriteLine("Ends with ' }}'>': " + result.EndsWith(" }>"));
Console.WriteLine("Starts with '<{{': " + result.StartsWith("<{"));
Console.WriteLine("Ends with '}}>': " + result.EndsWith("}>"));
