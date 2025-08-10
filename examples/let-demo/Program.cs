using System.Text;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;

namespace LetDemo;

public class Program {
    public static void Main(string[] args) {
        Console.OutputEncoding = Encoding.UTF8;
        var filePath = args.Length > 0 ? args[0] : "let-demo.xfer";
        if (!File.Exists(filePath)) {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }

        var parser = new Parser();
        var input = File.ReadAllText(filePath);
        XferDocument? doc = null;
        try {
            doc = parser.Parse(input); // Immediate substitution model: no final pass needed
        }
        catch (Exception ex) {
            Console.WriteLine("Parse error:");
            Console.WriteLine(ex.Message);
            if (ex.InnerException != null) {
                Console.WriteLine($"Inner: {ex.InnerException.Message}");
            }
            return;
        }

        if (doc.Warnings.Any()) {
            Console.WriteLine("Warnings:");
            foreach (var w in doc.Warnings) {
                Console.WriteLine($"  [{w.Type}] {w.Message} (row {w.Row}, col {w.Column})");
            }
            Console.WriteLine();
        }

        Console.WriteLine("=== Original Source ===");
        Console.WriteLine(input);
        Console.WriteLine();

        Console.WriteLine("=== Serialized After Parsing (let substitutions applied) ===");
        Console.WriteLine(doc.ToXfer());
        Console.WriteLine();

        // TEMP DEBUG: enumerate root tuple children and their runtime types
        if (doc.Root is TupleElement rootTuple) {
            Console.WriteLine("[Debug] Root tuple children:");
            for (int i = 0; i < rootTuple.Children.Count; i++) {
                var child = rootTuple.Children[i];
                Console.WriteLine($"  [{i}] {child.GetType().Name} => {child.ToXfer()}");
            }
            Console.WriteLine();
        }

        // Show elements that have IDs (if any were created inside the demo)
        if (doc.GetAllElementsWithIds().Count > 0) {
            Console.WriteLine("Elements with IDs:");
            foreach (var kv in doc.GetAllElementsWithIds()) {
                Console.WriteLine($"  {kv.Key}: {kv.Value.ToXfer()}");
            }
            Console.WriteLine();
        }

        Console.WriteLine("(Note) Direct reference lookup by name is not exposed via a public API; this demo focuses on how 'let' duplicates values in the serialized form above.");

        Console.WriteLine();
        Console.WriteLine("Demo complete.");
    }
}
