using Cliffer;

using ParksComputing.Xfer;
using ParksComputing.Xfer.Attributes;
using ParksComputing.Xfer.Services;
using ParksComputing.Xfer.Models.Elements;
using ParksComputing.Xfer.Extensions;

namespace ParksComputing.Xferc.Commands;

[Command("serialize", "Serialize an object to an Xfer document and display the result.")]
// [Argument(typeof(string), "file", "The path to the class file for which to serialize to Xfer")]
internal class SerializeCommand {
    public int Execute(string file) {
        var data = new SampleData {
            Person = new Person { Name = null, Age = 42 },
            CreatedAt = DateTime.UtcNow,
            Description = "Serializing Xfer makes me <\\$1F600\\>",
            ints = new int[] { 1, 2, 3 },
            strings = new string[] { "one", "two", "three" },
            bag_o_bits1 = new List<object> { "one", 2, 3.14 }
        };

        string xferDocument = XferConvert.Serialize(data);
        Console.WriteLine(xferDocument);

        var deserializedData = XferConvert.Deserialize<SampleData>(xferDocument);
        Console.WriteLine(deserializedData.Person.Name);
        Console.WriteLine(deserializedData.Person.Age);
        Console.WriteLine(deserializedData.CreatedAt);
        Console.WriteLine(deserializedData.Description);
        Console.WriteLine(string.Join(", ", deserializedData.ints));
        Console.WriteLine(string.Join(", ", deserializedData.strings));
        Console.WriteLine(string.Join(", ", deserializedData.bag_o_bits1));

        return Result.Success;
    }
}

public class Person {
    [XferProperty("Full name:")]
    public string Name { get; set; } = string.Empty;

    public int Age { get; set; }
}

public class SampleData {
    public Person Person { get; set; } = new Person { Name = "Alice", Age = 30 };

    public DateTime CreatedAt { get; set; }

    [XferEvaluated]
    public string? Description { get; set; }

    public int[] ints { get; set; } = [];
    public string[] strings { get; set; } = [];
    public List<object> bag_o_bits1 { get; set; } = new List<object>() { };
}
