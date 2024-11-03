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
            CreatedAt = DateTime.UtcNow
        };

        string xferDocument = XferConverter.Serialize(data);
        Console.WriteLine(xferDocument);

        var deserializedData = XferConverter.Deserialize<SampleData>(xferDocument);
        Console.WriteLine(deserializedData.Name);
        Console.WriteLine(deserializedData.Age);
        Console.WriteLine(deserializedData.CreatedAt);

        return Result.Success;
    }
}

public class SampleData {
    [XferProperty]
    public string Name { get; set; } = string.Empty;

    [XferProperty]
    public int Age { get; set; }

    [XferProperty("created_at")]
    public DateTime CreatedAt { get; set; }
}
