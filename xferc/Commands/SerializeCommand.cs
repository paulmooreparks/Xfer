using Cliffer;

using ParksComputing.Xfer;
using ParksComputing.Xfer.Attributes;
using ParksComputing.Xfer.Services;
using ParksComputing.Xfer.Extensions;
using ParksComputing.Xfer.Elements;

namespace ParksComputing.Xferc.Commands;

[Command("serialize", "Serialize an object to an Xfer document and display the result.")]
// [Argument(typeof(string), "file", "The path to the class file for which to serialize to Xfer")]
internal class SerializeCommand {
    public int Execute(string file) {
        var data = new SampleData {
        };

        string xferContent = XferConvert.Serialize(data, Formatting.Indented | Formatting.Spaced);
        Console.WriteLine(xferContent);

        var deserializedData = XferConvert.Deserialize<SampleData>(xferContent);
        Console.WriteLine(deserializedData.DateTimeOffset);

        var parser = new Parser();
        var document = parser.Parse(xferContent);
        Console.WriteLine(document.ToXfer(Formatting.Pretty));

        var x = document.Root[0];

        if (x is ObjectElement o) {
            if (o.TryGetElement("Dto", out DateTimeElement? element)) {
                Console.WriteLine(element?.ToXfer(Formatting.Pretty));
            }
        }

        return Result.Success;
    }
}

public class Person {
    [XferProperty("Full name:")]
    public string? Name { get; set; } = string.Empty;

    public int Age { get; set; }
}
