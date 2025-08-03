using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.ProcessingInstructions;
using ParksComputing.Xfer.Lang.Services;
using ParksComputing.Xfer.Lang;

namespace ExternalLogPiDemo;

/// <summary>
/// External Log Processing Instruction - completely independent from core library.
/// Logs target elements to various destinations with different formats.
/// </summary>
public class LogProcessingInstruction : ProcessingInstruction {
    public const string Keyword = "log";

    public string Level { get; private set; } = "info";
    public string Destination { get; private set; } = "console";
    public string Format { get; private set; } = "pretty";
    public string? Message { get; private set; }

    public LogProcessingInstruction(ObjectElement logConfig) : base(logConfig, Keyword) {
        Console.WriteLine("[LOG PI] Constructor called - creating LogProcessingInstruction");
        ParseConfiguration(logConfig);
        Console.WriteLine($"[LOG PI] Configuration parsed - Level: {Level}, Destination: {Destination}, Format: {Format}");
    }

    private void ParseConfiguration(ObjectElement config) {
        foreach (var kvp in config.Dictionary.Values) {
            switch (kvp.Key.ToLowerInvariant()) {
                case "level":
                    Level = ResolveValue(kvp.Value) ?? "info";
                    break;
                case "destination":
                    Destination = ResolveValue(kvp.Value) ?? "console";
                    break;
                case "format":
                    Format = ResolveValue(kvp.Value) ?? "pretty";
                    break;
                case "message":
                    Message = ResolveValue(kvp.Value);
                    break;
            }
        }
    }

    /// <summary>
    /// Recursively resolves an element value, handling simple strings, keyword-value pairs, and nested objects.
    /// Examples: "console" -> "console", file "logs/test.log" -> "file:logs/test.log"
    /// </summary>
    private string ResolveValue(Element element) {
        if (element is TextElement textElement) {
            return textElement.Value;
        }

        if (element is ObjectElement objElement) {
            // Handle nested objects like: { file "logs/test.log" }
            if (objElement.Dictionary.Count == 1) {
                var kvp = objElement.Dictionary.Values.First();
                var key = kvp.Key;
                var value = ResolveValue(kvp.Value);

                // Return combined format: "file:path" or just the key if no value
                return value != null ? $"{key}:{value}" : key;
            }
        }

        if (element is KeyValuePairElement kvpElement) {
            // Handle keyword-value pairs like: file "logs/test.log"
            var key = kvpElement.Key;
            var value = ResolveValue(kvpElement.Value);

            // Return combined format: "keyword:value"
            return $"{key}:{value}";
        }

        // Fallback to string representation
        return element.ToString();
    }

    public override void ProcessingInstructionHandler() {
        Console.WriteLine("[LOG PI] ProcessingInstructionHandler called");
        // Log PIs don't need global registration like dynamicSource
        // They operate on their target elements when encountered
    }

    public override void ElementHandler(Element element) {
        Console.WriteLine($"[LOG PI] ElementHandler called with: {element?.GetType().Name ?? "null"}");

        if (element == null) {
            Console.WriteLine("[LOG PI] Element is null, skipping.");
            return;
        }

        Console.WriteLine($"[LOG PI] Processing element: {element.GetType().Name}");

        try {
            LogElement(element);
        }
        catch (Exception ex) {
            Console.WriteLine($"[LOG ERROR] Failed to log element: {ex.Message}");
            Console.WriteLine($"[LOG ERROR] Stack trace: {ex.StackTrace}");
        }
    }

    private void LogElement(Element element) {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logEntry = CreateLogEntry(element, timestamp);

        if (Destination.StartsWith("file:", StringComparison.OrdinalIgnoreCase)) {
            LogToFile(logEntry, Destination.Substring(5));
        }
        else if (Destination.Equals("console", StringComparison.OrdinalIgnoreCase)) {
            LogToConsole(logEntry);
        }
        else {
            Console.WriteLine($"[LOG WARN] Unknown destination: {Destination}. Falling back to console.");
            LogToConsole(logEntry);
        }
    }

    private string CreateLogEntry(Element element, string timestamp) {
        var logData = new Dictionary<string, object> {
            ["timestamp"] = timestamp,
            ["level"] = Level.ToUpperInvariant(),
            ["element_type"] = element.GetType().Name
        };

        if (!string.IsNullOrEmpty(Message)) {
            logData["message"] = Message;
        }

        switch (Format.ToLowerInvariant()) {
            case "json":
                logData["content"] = element.ToXfer();
                return JsonSerializer.Serialize(logData, new JsonSerializerOptions { WriteIndented = true });

            case "compact":
                var compactContent = element.ToXfer().Replace("\n", " ").Replace("\r", "");
                if (compactContent.Length > 100) {
                    compactContent = compactContent.Substring(0, 97) + "...";
                }
                return $"[{timestamp}] {Level.ToUpperInvariant()}: {element.GetType().Name} - {compactContent}" +
                       (Message != null ? $" ({Message})" : "");

            case "pretty":
            default:
                var prettyContent = element.ToXfer(Formatting.Pretty);
                var header = $"[{timestamp}] {Level.ToUpperInvariant()}: {element.GetType().Name}" +
                           (Message != null ? $" - {Message}" : "");
                return $"{header}\n{prettyContent}\n{new string('-', 50)}";
        }
    }

    private void LogToConsole(string logEntry) {
        var originalColor = Console.ForegroundColor;

        try {
            Console.ForegroundColor = Level.ToLowerInvariant() switch {
                "debug" => ConsoleColor.Gray,
                "info" => ConsoleColor.Cyan,
                "warn" => ConsoleColor.Yellow,
                "error" => ConsoleColor.Red,
                _ => ConsoleColor.White
            };

            Console.WriteLine(logEntry);
        }
        finally {
            Console.ForegroundColor = originalColor;
        }
    }

    private void LogToFile(string logEntry, string filePath) {
        try {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
            File.AppendAllText(filePath, logEntry + Environment.NewLine);
            Console.WriteLine($"[LOG] Logged to file: {filePath}");
        }
        catch (Exception ex) {
            Console.WriteLine($"[LOG ERROR] Failed to write to file {filePath}: {ex.Message}");
            Console.WriteLine("Falling back to console output:");
            LogToConsole(logEntry);
        }
    }
}

/// <summary>
/// External PI Registry Extension - demonstrates how to register completely external PIs.
/// </summary>
public static class LogPiExtension {
    private static Parser? _registeredParser;

    /// <summary>
    /// Registers the Log PI with a parser instance's PI registry.
    /// </summary>
    public static void Register(Parser parser) {
        // Register the Log PI processor with the parser's PI registry
        parser.RegisterPIProcessor(LogProcessingInstruction.Keyword, CreateLogProcessingInstruction);
        _registeredParser = parser;
        Console.WriteLine("✓ Registered external 'log' Processing Instruction");
    }

    /// <summary>
    /// Unregisters the Log PI from the previously registered parser.
    /// </summary>
    public static void Unregister() {
        if (_registeredParser != null) {
            _registeredParser.UnregisterPIProcessor(LogProcessingInstruction.Keyword);
            _registeredParser = null;
            Console.WriteLine("✓ Unregistered external 'log' Processing Instruction");
        }
    }

    /// <summary>
    /// Factory method for creating LogProcessingInstruction instances.
    /// This matches the PIProcessor delegate signature.
    /// </summary>
    private static ProcessingInstruction CreateLogProcessingInstruction(KeyValuePairElement kvp, Parser parser) {
        if (kvp.Value is not ObjectElement obj) {
            throw new InvalidOperationException($"Log PI expects an object element. Found: {kvp.Value?.GetType().Name}");
        }

        return new LogProcessingInstruction(obj);
    }
}
