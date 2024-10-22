using Cliffer;

using System.CommandLine;
using ParksComputing.Xfer;
using ParksComputing.Xfer.Parser;

namespace ParksComputing.Xferc;

[Command("parse", "Parse and display an Xfer document.")]
[Argument(typeof(string), "file", "The path to the Xfer document")]
internal class ParseCommand {
    public int Execute(string file) {
        var input = File.ReadAllText(file);
        var parser = new XferParser();
        var result = parser.Parse(input);

        Console.WriteLine("Metadata:");
        foreach (var kvp in result.Metadata) {
            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("Content:");
        result.Content.ForEach(Console.WriteLine);

        return Result.Success;
    }
}
