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
    public const string Keyword = "dynamicSource";

    public DynamicSourceProcessingInstruction(ObjectElement sourceConfig) : base(sourceConfig, Keyword) { }

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

    public override void ElementHandler(Element element) {
        // The dynamicSource PI is document-level and handled in ProcessingInstructionHandler
        // No specific element-level handling needed here
    }
}
