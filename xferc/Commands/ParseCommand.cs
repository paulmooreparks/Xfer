using Cliffer;

using System.CommandLine;
using ParksComputing.Xfer;
using ParksComputing.Xfer.Services;

namespace ParksComputing.Xferc;

[Command("parse", "Parse and display an Xfer document.")]
[Argument(typeof(string), "file", "The path to the Xfer document")]
internal class ParseCommand {
    public int Execute(string file) {
        var input = File.ReadAllText(file);
        var parser = new Parser();
        var document = parser.Parse(input);

        Console.WriteLine(document);

        return Result.Success;
    }
}
