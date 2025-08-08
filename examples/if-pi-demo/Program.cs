using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.ProcessingInstructions;
using ParksComputing.Xfer.Lang.Services;
using ParksComputing.Xfer.Lang.DynamicSource;

namespace IfPiDemo;

/// <summary>
/// Demonstrates comprehensive usage of the If Processing Instruction (PI) in XferLang.
/// Shows how conditional elements work with various expression types and dynamic sources.
/// </summary>
public class Program {
    private static int _totalConditions = 0;
    private static int _conditionsMet = 0;
    private static int _conditionsFailed = 0;

    public static void Main(string[] args) {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("=== If Processing Instruction Demo ===");
        Console.WriteLine();
        Console.WriteLine("This demo shows how the <! if !> processing instruction works");
        Console.WriteLine("with various types of conditions and expressions in XferLang.");
        Console.WriteLine();

        try {
            // Set up dynamic source resolver with test variables
            var resolver = SetupDynamicSourceResolver();

            // Get list of .xfer files to process
            var xferFiles = GetXferFiles();

            if (xferFiles.Count == 0) {
                Console.WriteLine("❌ No .xfer files found in the current directory.");
                return;
            }

            // Process each .xfer file
            foreach (var file in xferFiles) {
                ProcessXferFile(file, resolver);
                Console.WriteLine();
            }

            // Show summary
            ShowSummary();
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ Error during demo: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Sets up a dynamic source resolver with predefined test variables.
    /// </summary>
    private static IDynamicSourceResolver SetupDynamicSourceResolver() {
        // Setup the dynamic source registry with test variables
        var dynamicSources = new Dictionary<string, Element> {
            // Environment variables
            ["ENVIRONMENT"] = new StringElement("development"),
            ["PLATFORM"] = new StringElement("Linux"),
            ["DEBUG_MODE"] = new StringElement("true"),
            ["LOG_LEVEL"] = new StringElement("verbose"),

            // Variables for testing existence vs truthiness distinction
            ["EMPTY_STRING"] = new StringElement(""),          // Exists but falsy
            ["FALSE_BOOLEAN"] = new BooleanElement(false),     // Exists but falsy
            ["FEATURE_TOGGLE"] = new BooleanElement(false),    // Exists but disabled

            // System specifications
            ["PROCESSORS"] = new IntegerElement(8),
            ["MEMORY_GB"] = new IntegerElement(16),
            ["DISK_SPACE_GB"] = new IntegerElement(500),

            // Application settings
            ["SCORE"] = new IntegerElement(85),
            ["AGE"] = new IntegerElement(25),
            ["USER_AGE"] = new IntegerElement(32),
            ["TEMPERATURE"] = new DecimalElement(22.5m),
            ["FEATURE_FLAGS"] = new StringElement("new_ui,dark_mode"),
            ["IS_ADMIN"] = new BooleanElement(true),
            ["USERNAME"] = new StringElement("demo_user"),

            // Numeric comparisons
            ["CURRENT_VERSION"] = new StringElement("2.1.0"),
            ["MIN_VERSION"] = new StringElement("1.5.0"),
            ["MAX_CONNECTIONS"] = new IntegerElement(100),
            ["CURRENT_CONNECTIONS"] = new IntegerElement(45),

            // Boolean flags
            ["SSL_ENABLED"] = new BooleanElement(true),
            ["MAINTENANCE_MODE"] = new BooleanElement(false),
            ["ENABLE_NEW_FEATURE"] = new BooleanElement(true),
            ["DEBUG_LOGGING"] = new BooleanElement(false)
        };

        DynamicSourceRegistry.SetConfigurations(dynamicSources);
        return new DefaultDynamicSourceResolver();
    }

    /// <summary>
    /// Finds all processing instructions recursively within an element and its children.
    /// </summary>
    private static List<ProcessingInstruction> FindAllProcessingInstructions(Element? element) {
        var pis = new List<ProcessingInstruction>();

        if (element == null) {
            return pis;
        }

        // Check if this element is a PI
        if (element is ProcessingInstruction pi) {
            pis.Add(pi);
        }

        // Recursively search children
        foreach (var child in element.Children) {
            pis.AddRange(FindAllProcessingInstructions(child));
        }

        return pis;
    }

    /// <summary>
    /// Gets a list of .xfer files in the current directory, sorted by name.
    /// </summary>
    private static List<string> GetXferFiles() {
        var files = new List<string>();
        var currentDir = Directory.GetCurrentDirectory();

        foreach (var file in Directory.GetFiles(currentDir, "*.xfer")) {
            files.Add(Path.GetFileName(file));
        }

        files.Sort();
        return files;
    }

    /// <summary>
    /// Processes a single .xfer file and demonstrates If PI functionality.
    /// </summary>
    private static void ProcessXferFile(string filename, IDynamicSourceResolver resolver) {
        Console.WriteLine($"📄 Processing: {filename}");
        Console.WriteLine(new string('─', 50));

        try {
            if (!File.Exists(filename)) {
                Console.WriteLine($"❌ File not found: {filename}");
                return;
            }

            var content = File.ReadAllText(filename);
            Console.WriteLine($"📝 Content preview:");
            ShowContentPreview(content);
            Console.WriteLine();

            // Create parser and register If PI
            var parser = new Parser();
            RegisterIfProcessingInstruction(parser);

            // Set up dynamic source resolver
            parser.DynamicSourceResolver = resolver;

            // Parse the document
            var doc = parser.Parse(content);
            Console.WriteLine("✓ Document parsed successfully");
            Console.WriteLine($"✓ Found {doc.ProcessingInstructions.Count} document-level processing instructions");

            // Look for PIs within the document structure (not just document-level)
            var allPIs = FindAllProcessingInstructions(doc.Root);
            Console.WriteLine($"📋 Found {allPIs.Count} processing instructions in document structure");

            // The PIs should have been processed automatically during parsing
            // Let's just display their status for informational purposes
            foreach (var pi in allPIs) {
                if (pi is IfProcessingInstruction ifPI) {
                    DisplayIfInstructionStatus(ifPI);
                }
            }

            // DEBUG: Let's see what elements we actually have AFTER PI processing
            Console.WriteLine($"📋 Root element type: {doc.Root.GetType().Name}");
            Console.WriteLine($"📋 Root element count AFTER PI processing: {doc.Root.Count}");
            if (doc.Root.Count > 0) {
                for (int i = 0; i < Math.Min(5, doc.Root.Count); i++) {
                    var element = doc.Root.GetElementAt(i);
                    var preview = element.ToXfer().Length > 50 ? element.ToXfer().Substring(0, 50) + "..." : element.ToXfer();
                    Console.WriteLine($"   - {element.GetType().Name}: {preview}");
                }
            }

            // Show document structure if it has conditional elements
            if (doc.ProcessingInstructions.Count > 0) {
                Console.WriteLine();
                Console.WriteLine("📋 Document structure:");
                ShowDocumentStructure(doc);
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ Error processing {filename}: {ex.Message}");
            if (ex.InnerException != null) {
                Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
            }
        }
    }

    /// <summary>
    /// Registers the If Processing Instruction with the parser.
    /// </summary>
    private static void RegisterIfProcessingInstruction(Parser parser) {
        parser.RegisterPIProcessor(IfProcessingInstruction.Keyword, (kvp, p) => {
            return new IfProcessingInstruction(kvp.Value);
        });
    }

    /// <summary>
    /// Displays the status of an If PI without triggering ElementHandler again.
    /// </summary>
    private static void DisplayIfInstructionStatus(IfProcessingInstruction ifPI) {
        _totalConditions++;

        var conditionText = GetConditionText(ifPI.ConditionExpression);
        var resultIcon = ifPI.ConditionMet ? "✓" : "✗";
        var resultText = ifPI.ConditionMet ? "true" : "false";
        var statusText = ifPI.ConditionMet ? "condition met" : "condition failed";

        Console.WriteLine($"{resultIcon} <! if {conditionText} !> → {resultText} ({statusText})");

        if (ifPI.ConditionMet) {
            _conditionsMet++;
        } else {
            _conditionsFailed++;
        }

        // Show target element if available
        if (ifPI.Target != null) {
            var targetPreview = GetElementPreview(ifPI.Target);
            var targetStatus = ifPI.ConditionMet ? "PROCESSED" : "REMOVED";
            Console.WriteLine($"   Target: {targetPreview} [{targetStatus}]");
        }
    }

    /// <summary>
    /// Processes and displays results for an If PI instance.
    /// </summary>
    private static void ProcessIfInstruction(IfProcessingInstruction ifPI) {
        _totalConditions++;

        var conditionText = GetConditionText(ifPI.ConditionExpression);
        var resultIcon = ifPI.ConditionMet ? "✓" : "✗";
        var resultText = ifPI.ConditionMet ? "true" : "false";
        var statusText = ifPI.ConditionMet ? "condition met" : "condition failed";

        Console.WriteLine($"{resultIcon} <! if {conditionText} !> → {resultText} ({statusText})");

        if (ifPI.ConditionMet) {
            _conditionsMet++;
        } else {
            _conditionsFailed++;
        }

        // Show target element if available
        if (ifPI.Target != null) {
            var targetPreview = GetElementPreview(ifPI.Target);
            var targetStatus = ifPI.ConditionMet ? "PROCESSED" : "CONDITIONAL";
            Console.WriteLine($"   Target: {targetPreview} [{targetStatus}]");
        }
    }

    /// <summary>
    /// Gets a readable text representation of a condition expression.
    /// </summary>
    private static string GetConditionText(Element condition) {
        if (condition is DynamicElement dynamicElement) {
            return $"<|{dynamicElement.Value}|>";
        }

        if (condition is CollectionElement collection && collection.Count >= 2) {
            var operatorElement = collection.GetElementAt(0);
            if (operatorElement is TextElement textElement) {
                var operatorName = textElement.Value;
                var args = new List<string>();

                for (int i = 1; i < collection.Count; i++) {
                    var arg = collection.GetElementAt(i);
                    args.Add(GetElementPreview(arg));
                }

                return $"{operatorName}[{string.Join(" ", args)}]";
            }
        }

        return GetElementPreview(condition);
    }

    /// <summary>
    /// Gets a short preview of an element for display purposes.
    /// </summary>
    private static string GetElementPreview(Element? element) {
        if (element == null) return "null";

        // Handle numeric elements by checking if ToXfer() starts with #
        if (element.GetType().Name.EndsWith("Element") &&
            element.GetType().BaseType?.Name.StartsWith("NumericElement") == true) {
            return element.ToXfer();
        }

        return element switch {
            DynamicElement dynamic => $"<|{dynamic.Value}|>",
            BooleanElement boolean => $"~{boolean.Value}",
            ObjectElement obj => $"{{{obj.Dictionary.Count} props}}",
            CollectionElement coll => $"[{coll.Count} items]",
            TextElement text => $"\"{text.Value}\"",
            _ => element.GetType().Name
        };
    }

    /// <summary>
    /// Shows a preview of the file content.
    /// </summary>
    private static void ShowContentPreview(string content) {
        var lines = content.Split('\n');
        var maxLines = Math.Min(5, lines.Length);

        for (int i = 0; i < maxLines; i++) {
            var line = lines[i].Trim();
            if (!string.IsNullOrEmpty(line)) {
                Console.WriteLine($"   {line}");
            }
        }

        if (lines.Length > maxLines) {
            Console.WriteLine($"   ... ({lines.Length - maxLines} more lines)");
        }
    }

    /// <summary>
    /// Shows the structure of a parsed document.
    /// </summary>
    private static void ShowDocumentStructure(XferDocument doc) {
        if (doc.Root != null) {
            Console.WriteLine($"   Root: {GetElementPreview(doc.Root)}");
        }

        if (doc.ProcessingInstructions.Count > 0) {
            Console.WriteLine($"   PIs: {doc.ProcessingInstructions.Count} instructions");
        }

        // Show any elements with conditional state
        var conditionalElements = FindConditionalElements(doc);
        if (conditionalElements.Count > 0) {
            Console.WriteLine($"   Conditional elements: {conditionalElements.Count}");
        }
    }

    /// <summary>
    /// Finds elements that have been marked with conditional state.
    /// </summary>
    private static List<Element> FindConditionalElements(XferDocument doc) {
        var conditionalElements = new List<Element>();

        // This is a simplified implementation - in practice, you'd recursively
        // traverse the document tree to find all conditional elements
        if (doc.Root != null && IsConditionalElement(doc.Root)) {
            conditionalElements.Add(doc.Root);
        }

        return conditionalElements;
    }

    /// <summary>
    /// Checks if an element has been marked as conditional.
    /// </summary>
    private static bool IsConditionalElement(Element element) {
        // In the current IfProcessingInstruction implementation,
        // conditional elements get their ID modified to include ":conditional-false"
        return element.Id?.Contains("conditional") == true;
    }

    /// <summary>
    /// Shows a summary of the demo results.
    /// </summary>
    private static void ShowSummary() {
        Console.WriteLine("🎯 Demo Summary");
        Console.WriteLine(new string('═', 50));
        Console.WriteLine($"Total conditions processed: {_totalConditions}");
        Console.WriteLine($"✓ Conditions met: {_conditionsMet}");
        Console.WriteLine($"✗ Conditions failed: {_conditionsFailed}");

        if (_totalConditions > 0) {
            var successRate = (_conditionsMet * 100.0) / _totalConditions;
            Console.WriteLine($"Success rate: {successRate:F1}%");
        }

        Console.WriteLine();
        Console.WriteLine("✅ If Processing Instruction demo completed successfully!");
        Console.WriteLine();
        Console.WriteLine("💡 Key takeaways:");
        Console.WriteLine("   • If PIs evaluate conditions and affect target elements");
        Console.WriteLine("   • Conditions can be simple variables or complex expressions");
        Console.WriteLine("   • Invalid conditions gracefully default to false");
        Console.WriteLine("   • Conditional state is preserved in element metadata");
        Console.WriteLine("   • Integration with dynamic sources enables environment-specific configs");
    }
}
