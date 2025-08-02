using System.CommandLine;
using System.Text;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;
using ParksComputing.Xfer.Lang.Extensions;

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
        if (deserializedData != null)
        {
            Console.WriteLine(deserializedData.DateTimeOffset);
        }

        var parser = new Parser();
        var document = parser.Parse(xferContent);
        if (document != null)
        {
            Console.WriteLine(document.ToXfer(Formatting.Pretty));

            var x = document.Root.Values.FirstOrDefault();

            if (x is ObjectElement o)
            {
                if (o.TryGetElement("DateTimeOffset", out DateTimeElement? element))
                {
                    Console.WriteLine(element?.ToXfer(Formatting.Pretty));
                }
            }
        }

        return Result.Success;
    }
}
