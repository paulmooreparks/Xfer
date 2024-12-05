using System.CommandLine;
using System.Text;
using ParksComputing.Xfer;
using ParksComputing.Xfer.Services;
using ParksComputing.Xfer.Extensions;

using Cliffer;

namespace ParksComputing.Xferc.Commands;

[Command("test", "Random Xfer test code")]
internal class TestCommand {
    public int Execute() {
        var file = "..\\..\\..\\..\\test.xfer";
        var inputBytes = File.ReadAllBytes(file);
        var parser = new Parser();
        var document = parser.Parse(inputBytes);

        Console.WriteLine($"Document uses Xfer version {document.Metadata.Xfer}");
        // Console.WriteLine($"Message ID is {document.Metadata.MessageId}");
        Console.WriteLine();

        var xfer = document.Root.ToXfer(Formatting.Pretty);
        Console.WriteLine(xfer);

        return Result.Success;
    }
}
