using Cliffer;

using ParksComputing.Xfer;
using ParksComputing.Xfer.Attributes;
using ParksComputing.Xfer.Services;
using ParksComputing.Xfer.Models.Elements;

namespace ParksComputing.Xferc.Commands;

[Command("serialize", "Serialize an object to an Xfer document and display the result.")]
// [Argument(typeof(string), "file", "The path to the class file for which to serialize to Xfer")]
internal class SerializeCommand {
    public int Execute(string file) {
        var data = new SampleData {
            Name = "John Doe",
            Age = 42,
            CreatedAt = DateTime.UtcNow,
            Description = "This is a description.",
            ints = new int[] { 1, 2, 3 },
            strings = new string[] { "one", "two", "three" },
            bag_o_bits = new List<object> { "one", 2, 3.14 }
        };

        string xferDocument = XferConverter.Serialize(data);
        Console.WriteLine(xferDocument);

        var deserializedData = XferConverter.Deserialize<SampleData>(xferDocument);
        Console.WriteLine(deserializedData.Name);
        Console.WriteLine(deserializedData.Age);
        Console.WriteLine(deserializedData.CreatedAt);
        Console.WriteLine(deserializedData.Description);
        Console.WriteLine(string.Join(", ", deserializedData.ints));
        Console.WriteLine(string.Join(", ", deserializedData.strings));
        Console.WriteLine(string.Join(", ", deserializedData.bag_o_bits));

        return Result.Success;
    }
}

public class SampleData {
    public string Name { get; set; } = string.Empty;

    [XferProperty]
    public int Age { get; set; }

    [XferProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [XferLiteral]
    public string? Description { get; set; }

    public int[] ints { get; set; } = [];
    public string[] strings { get; set; } = [];
    public List<object> bag_o_bits { get; set; } = new List<object>() { };
}
