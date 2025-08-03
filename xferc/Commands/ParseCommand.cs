using Cliffer;

using System.CommandLine;
using System.Text;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Services;
using ParksComputing.Xfer.Lang.Extensions;
using ParksComputing.Xfer.Lang.ProcessingInstructions;

namespace ParksComputing.Xferc;

[Command("parse", "Parse and display an Xfer document.")]
[Argument(typeof(string), "file", "The path to the Xfer document", Cliffer.ArgumentArity.ZeroOrOne)]
internal class ParseCommand {
    public int Execute(string file) {
        /* TODO: Temporary... */
        // file = "../../../../schemas/address.xfer";
        // file = "../../../../xfertest.xfer";
        var inputBytes = File.ReadAllBytes(file);
        var parser = new Parser();
        var document = parser.Parse(inputBytes);

        // Check for errors and warnings
        if (document.HasError) {
            Console.WriteLine($"Parse Error: {document.Error}");
            return 1; // Error exit code
        }

        if (document.HasWarnings) {
            Console.WriteLine($"Parse Warnings ({document.Warnings.Count}):");
            foreach (var warning in document.Warnings) {
                Console.WriteLine($"  {warning}");
            }
            Console.WriteLine();
        }

        // Find Xfer version from metadata in Root
        var xferVersion = document.Root.Children
            .OfType<ProcessingInstruction>()
            .Select(m => m.Kvp)
            .Where(kvp => kvp != null && kvp.Key.Equals("xfer", StringComparison.OrdinalIgnoreCase))
            .Select(kvp => kvp?.Value?.ToString())
            .FirstOrDefault();
        Console.WriteLine($"Document uses Xfer version {xferVersion}");
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
