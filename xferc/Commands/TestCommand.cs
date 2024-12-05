using System.CommandLine;
using System.Text;
using ParksComputing.Xfer;
using ParksComputing.Xfer.Elements;
using ParksComputing.Xfer.Services;
using ParksComputing.Xfer.Extensions;

using Cliffer;

namespace ParksComputing.Xferc.Commands;

[Command("test", "Random Xfer test code")]
internal class TestCommand {
    public int Execute() {
        var data = new SampleData {
        };

        string dateOnlyString = "2024-12-05";
        if (DateOnly.TryParse(dateOnlyString, out DateOnly dateOnly)) {
            Console.WriteLine(dateOnly.ToString("O"));
        }
        else {
            Console.WriteLine("Can't parse dateOnlyString");
        }

        string xferContent = XferConvert.Serialize(data, Formatting.Pretty);
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
