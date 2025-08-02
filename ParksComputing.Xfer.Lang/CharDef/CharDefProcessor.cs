using System;
using System.Collections.Generic;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.CharDef;
/// <summary>
/// Handles charDef PI processing and efficient lookup for CharacterElements.
/// Register with Parser.RegisterPIProcessor and RegisterElementProcessor.
/// </summary>
public class CharDefProcessor {
    // Single global charDef registry (static)

    public const string CharDefKey = "charDef";

    /// <summary>
    /// Update the global charDef registry with a charDef PI object.
    /// </summary>
    public static void UpdateGlobalRegistryFromPI(KeyValuePairElement charDefKvp) {
        var charDefRegistry = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (!string.Equals(charDefKvp.Key, CharDefKey, StringComparison.OrdinalIgnoreCase)) {
            return;
        }
        if (charDefKvp.Value is ObjectElement obj) {
            foreach (var kv in obj.Dictionary.Values) {
                var name = kv.KeyElement.ToString();
                if (kv.Value is CharacterElement charElem) {
                    charDefRegistry[name] = charElem.Value;
                }
                else {
                    throw new InvalidOperationException($"{CharDefKey} processing instruction expects a character element for key '{name}'. Found: {kv.Value.GetType().Name}");
                }
            }

            CharacterIdRegistry.SetCustomIds(charDefRegistry);
        }
    }

    public void PIHandler(KeyValuePairElement charDefKvp) {
        UpdateGlobalRegistryFromPI(charDefKvp);
    }

    public void ElementHandler(Element element) {
    }
}
