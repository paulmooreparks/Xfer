using Cliffer;

using System.CommandLine;
using System.Text;
using ParksComputing.Xfer;
using ParksComputing.Xfer.Services;
using ParksComputing.Xfer.Extensions;

namespace ParksComputing.Xferc;

[Command("parse", "Parse and display an Xfer document.")]
[Argument(typeof(string), "file", "The path to the Xfer document", Cliffer.ArgumentArity.ZeroOrOne)]
internal class ParseCommand {
    public int Execute(string file) {
        /* TODO: Temporary... */
        file = "..\\..\\..\\..\\schemas\\address.xfer";
        var inputBytes = File.ReadAllBytes(file);
        var parser = new Parser();
        var document = parser.Parse(inputBytes);

        Console.WriteLine($"Document uses Xfer version {document.Metadata.Xfer}");
        // Console.WriteLine($"Message ID is {document.Metadata.MessageId}");
        Console.WriteLine();

        Console.WriteLine(document.ToXfer(Formatting.Indented | Formatting.Spaced));

#if false
        using (var stringWriter = new StringWriter())
        using (var xferWriter = new XferTextWriter(stringWriter) { Formatting = Formatting.Indented }) {
            xferWriter.Write(document);
            Console.WriteLine(stringWriter.ToString());
        }
#endif

        return Result.Success;
    }
}

internal abstract class XElement<T> {
    internal T Value { get; set; }

    internal XElement(T value) { Value = value; } 
}

internal class IntegerXElement : XElement<int> {
    internal IntegerXElement(int value) : base(value) {
        Value = value;
    }
}
