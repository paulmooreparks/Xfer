using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions;

/// <summary>
/// Processing instruction for configuring dynamic data sources in XferLang.
/// Allows specification of external data sources, connection settings, and
/// dynamic content resolution strategies. The instruction expects an object
/// containing source configuration parameters.
/// </summary>
public class DynamicSourceProcessingInstruction : ProcessingInstruction {
    /// <summary>
    /// The keyword used to identify dynamic source processing instructions.
    /// </summary>
    public const string Keyword = "dynamicSource";

    /// <summary>
    /// Initializes a new instance of the DynamicSourceProcessingInstruction class with the specified source configuration.
    /// </summary>
    /// <param name="sourceConfig">The object element containing dynamic source configuration parameters.</param>
    public DynamicSourceProcessingInstruction(ObjectElement sourceConfig) : base(sourceConfig, Keyword) { }

    /// <summary>
    /// Handles the processing of the dynamic source configuration by registering all source configurations globally.
    /// This makes the configurations available during parsing for dynamic content resolution.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the value is not an ObjectElement.</exception>
    public override void ProcessingInstructionHandler() {
        if (Value is not ObjectElement obj) {
            throw new InvalidOperationException($"{Keyword} processing instruction expects an object element. Found: {Value.GetType().Name}");
        }

        // Register all source configurations globally so they're available during parsing
        var configurations = new Dictionary<string, Element>(StringComparer.OrdinalIgnoreCase);

        foreach (var kv in obj.Dictionary) {
            var key = kv.Value.Key;
            var sourceConfig = kv.Value.Value;
            configurations[key] = sourceConfig;
        }

        DynamicSourceRegistry.SetConfigurations(configurations);
    }

    /// <summary>
    /// Handles element-level processing for dynamic source instructions.
    /// Since dynamic source processing instructions are document-level and handled in ProcessingInstructionHandler,
    /// no specific element-level handling is needed.
    /// </summary>
    /// <param name="element">The element to be processed (unused for dynamic source instructions).</param>
    public override void ElementHandler(Element element) {
        // The dynamicSource PI is document-level and handled in ProcessingInstructionHandler
        // No specific element-level handling needed here
    }
}
