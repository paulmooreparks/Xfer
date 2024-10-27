using Cliffer;

using System.CommandLine;
using ParksComputing.Xfer;
using ParksComputing.Xfer.Services;
using ParksComputing.Xfer.Models.Elements;

namespace ParksComputing.Xferc;

[Command("parse", "Parse and display an Xfer document.")]
[Argument(typeof(string), "file", "The path to the Xfer document")]
internal class ParseCommand {
    public int Execute(string file) {
        var input = File.ReadAllText(file);
        var parser = new Parser();
        var document = parser.Parse(input);

        Console.WriteLine(document);

        Console.WriteLine(document.Metadata["foo"]);
        Console.WriteLine(document.Metadata["encoding"]);
        Console.WriteLine(document.Metadata.Encoding);

        document.Metadata["encoding"] = new StringElement("UTF-16");
        Console.WriteLine(document.Metadata["encoding"]);
        Console.WriteLine(document.Metadata.Encoding);

        document.Metadata.Encoding = "UTF-32";
        Console.WriteLine(document.Metadata["encoding"]);
        Console.WriteLine(document.Metadata.Encoding);

        Console.WriteLine(document.Metadata.Version);

        return Result.Success;
    }
}
