using System;
using System.IO;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Services;

namespace ExternalLogPiDemo;

class Program {
    static void Main(string[] args) {
        Console.WriteLine("=== External Log Processing Instruction Demo ===\n");

        try {
            // Demo 1: Parse and execute the sample.xfer file
            Console.WriteLine("Demo 1: Processing sample.xfer with various log configurations...\n");
            ProcessSampleFile();

            Console.WriteLine("\nDemo 2: Creating and processing an in-memory document...\n");
            ProcessInMemoryDocument();

            Console.WriteLine("\nDemo 3: Testing different log levels and formats...\n");
            TestDifferentConfigurations();
        }
        catch (Exception ex) {
            Console.WriteLine($"\n❌ Error during demo: {ex.Message}");
        }
    }

    static void ProcessSampleFile() {
        var samplePath = "sample.xfer";
        if (!File.Exists(samplePath)) {
            Console.WriteLine($"❌ Sample file not found: {samplePath}");
            return;
        }

        try {
            var content = File.ReadAllText(samplePath);
            Console.WriteLine($"📄 Processing file: {samplePath}");
            Console.WriteLine($"File content:\n{content}\n");

            // Create parser and register our Log PI
            var parser = new Parser();
            LogPiExtension.Register(parser);

            var doc = parser.Parse(content);
            Console.WriteLine("✓ Document parsed successfully");
            Console.WriteLine($"✓ Found {doc.ProcessingInstructions.Count} processing instructions");
            Console.WriteLine($"✓ Root element type: {doc.Root?.GetType().Name}");

            // Manual ElementHandler invocation for document-level PIs
            foreach (var pi in doc.ProcessingInstructions) {
                if (pi is LogProcessingInstruction logPI && pi.Target != null) {
                    Console.WriteLine($"Executing log PI: {logPI.Level} -> {logPI.Destination}");
                    logPI.ElementHandler(pi.Target);
                }
            }

            // Clean up
            LogPiExtension.Unregister();

            Console.WriteLine("✓ Log PIs executed successfully\n");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ Failed to process sample file: {ex.Message}");
        }
    }

    static void ProcessInMemoryDocument() {
        var xferContent = @"<!
log { level ""debug"" destination ""console"" format ""json"" message ""In-memory demo"" }
!>
{
    name ""John Doe""
    email ""john@example.com""
    preferences {
        theme ""dark""
        notifications true
    }
}";

        Console.WriteLine("📝 Processing in-memory XFER content:");
        Console.WriteLine($"{xferContent}\n");

        try {
            // Create parser and register our Log PI
            var parser = new Parser();
            LogPiExtension.Register(parser);

            var doc = parser.Parse(xferContent);
            Console.WriteLine("✓ In-memory document processed successfully");

            // Manual ElementHandler invocation for document-level PIs
            foreach (var pi in doc.ProcessingInstructions) {
                if (pi is LogProcessingInstruction logPI && pi.Target != null) {
                    Console.WriteLine($"Executing log PI: {logPI.Level} -> {logPI.Destination}");
                    logPI.ElementHandler(pi.Target);
                }
            }

            // Clean up
            LogPiExtension.Unregister();
            Console.WriteLine();
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ Failed to process in-memory document: {ex.Message}");
        }
    }

    static void TestDifferentConfigurations() {
        var configurations = new[] {
            // Error level with file output using recursive KVP pattern
            @"<!
            log { level ""error"" destination { file ""logs/error.log"" } format ""json"" }
            !>
            { code 500 message ""Internal Server Error"" }",

            // Warning with compact format - mix of simple and recursive patterns
            @"<!
            log { level ""warn"" destination ""console"" format ""compact"" message ""Deprecated API"" }
            !>
            { deprecated true replacement ""/api/v2/users"" }",

            // Info with pretty format using simple console destination
            @"<!
            log { level ""info"" destination ""console"" format ""pretty"" }
            !>
            { status ""ok"" data { count 42 } }"
        };

        for (int i = 0; i < configurations.Length; i++) {
            Console.WriteLine($"Configuration {i + 1}:");
            Console.WriteLine($"{configurations[i]}\n");

            try {
                // Create parser and register our Log PI
                var parser = new Parser();
                LogPiExtension.Register(parser);

            var doc = parser.Parse(configurations[i]);
            Console.WriteLine($"✓ Configuration {i + 1} processed");

            // Manually invoke ElementHandler for document-level PIs
            foreach (var pi in doc.ProcessingInstructions) {
                if (pi is LogProcessingInstruction logPI && pi.Target != null) {
                    Console.WriteLine($"Manually invoking ElementHandler for {pi.Kvp?.Key}...");
                    logPI.ElementHandler(pi.Target);
                }
            }
            Console.WriteLine();                // Clean up
                LogPiExtension.Unregister();
            }
            catch (Exception ex) {
                Console.WriteLine($"❌ Configuration {i + 1} failed: {ex.Message}\n");
            }
        }

        // Check if log files were created
        var logDir = "logs";
        if (Directory.Exists(logDir)) {
            var logFiles = Directory.GetFiles(logDir, "*.log");
            Console.WriteLine($"📁 Created {logFiles.Length} log file(s) in '{logDir}' directory:");
            foreach (var file in logFiles) {
                var size = new FileInfo(file).Length;
                Console.WriteLine($"  - {Path.GetFileName(file)} ({size} bytes)");
            }
        }
    }
}
