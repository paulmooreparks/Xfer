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

        var timeSpanString = TimeSpan.FromHours(1).ToString();
        Console.WriteLine(timeSpanString);

        string xferContent = XferConvert.Serialize(data, Formatting.Pretty);
        Console.WriteLine(xferContent);

        var deserializedData = XferConvert.Deserialize<SampleData>(xferContent);
        Console.WriteLine(deserializedData.DateTimeOffset);

        var parser = new Parser();
        var document = parser.Parse(xferContent);
        Console.WriteLine(document.ToXfer(Formatting.Pretty));

        var x = document.Root[0];

        if (x is ObjectElement o) {
            if (o.TryGetElement("DateTimeOffset", out DateTimeElement? element)) {
                Console.WriteLine(element?.ToXfer(Formatting.Pretty));
            }
        }

        return Result.Success;
    }
}
