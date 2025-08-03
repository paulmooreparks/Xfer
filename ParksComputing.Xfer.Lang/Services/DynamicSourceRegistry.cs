using System;
using System.Collections.Generic;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Services;

/// <summary>
/// Global registry for dynamic source configurations from dynamicSource PIs.
/// This allows dynamicSource PIs to affect the entire document scope.
/// </summary>
public static class DynamicSourceRegistry {
    private static Dictionary<string, Element> _configurations = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Sets the dynamic source configurations from a dynamicSource PI.
    /// </summary>
    /// <param name="configurations">The source configurations to register</param>
    public static void SetConfigurations(Dictionary<string, Element> configurations) {
        _configurations = new Dictionary<string, Element>(configurations, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Adds or updates a single dynamic source configuration.
    /// </summary>
    /// <param name="key">The dynamic element key</param>
    /// <param name="sourceConfig">The source configuration element</param>
    public static void AddConfiguration(string key, Element sourceConfig) {
        _configurations[key] = sourceConfig;
    }

    /// <summary>
    /// Gets the source configuration for a dynamic element key.
    /// </summary>
    /// <param name="key">The dynamic element key to lookup</param>
    /// <returns>The source configuration element, or null if not found</returns>
    public static Element? GetConfiguration(string key) {
        return _configurations.TryGetValue(key, out var config) ? config : null;
    }

    /// <summary>
    /// Clears all dynamic source configurations.
    /// Useful for starting fresh between document parses.
    /// </summary>
    public static void Clear() {
        _configurations.Clear();
    }

    /// <summary>
    /// Resolves a dynamic element key using the configured source and registered handlers.
    /// </summary>
    /// <param name="key">The dynamic element key to resolve</param>
    /// <returns>The resolved value, or null if not found or cannot be resolved</returns>
    public static string? Resolve(string key) {
        var sourceConfig = GetConfiguration(key);

        if (sourceConfig == null) {
            return null; // No configuration for this key
        }

        // Handle recursive KVP format: key sourceType "sourceValue"
        if (sourceConfig is KeyValuePairElement sourceKvp) {
            var sourceType = sourceKvp.Key; // e.g., "db", "env", "const"
            var sourceValue = sourceKvp.Value?.ToString(); // e.g., "dbpassword", "USERNAME", etc.

            // Delegate to the handler registry
            return DynamicSourceHandlerRegistry.Resolve(sourceType, sourceValue, key);
        }
        else {
            // Handle direct string values as literal constants
            return sourceConfig.ToString();
        }
    }

    /// <summary>
    /// Gets all registered configurations (read-only).
    /// </summary>
    public static IReadOnlyDictionary<string, Element> Configurations => _configurations;
}
