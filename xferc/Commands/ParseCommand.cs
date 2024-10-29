using Cliffer;

using System.CommandLine;
using System.Text;
using ParksComputing.Xfer;
using ParksComputing.Xfer.Services;
using ParksComputing.Xfer.Models.Elements;

namespace ParksComputing.Xferc;

[Command("parse", "Parse and display an Xfer document.")]
[Argument(typeof(string), "file", "The path to the Xfer document")]
internal class ParseCommand {
    public int Execute(string file) {
        var inputBytes = File.ReadAllBytes(file);
        var parser = new Parser();
        var document = parser.Parse(inputBytes);

        Console.WriteLine(document);

        return Result.Success;
    }
}
