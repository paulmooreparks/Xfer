using System;
using System.Collections.Generic;
using System.IO;

namespace ParksComputing.Xfer.Lang.Services;

/// <summary>
/// Handler delegate for resolving dynamic source values by source type.
/// </summary>
/// <param name="sourceValue">The source-specific value (e.g., environment variable name, database key)</param>
/// <param name="fallbackKey">The original dynamic element key to use as fallback</param>
/// <returns>The resolved value, or null if the handler cannot resolve it</returns>
public delegate string? DynamicSourceHandler(string? sourceValue, string fallbackKey);

/// <summary>
/// Registry for dynamic source type handlers, allowing extensions to register custom source types.
/// </summary>
public static class DynamicSourceHandlerRegistry {
    private static readonly Dictionary<string, DynamicSourceHandler> _handlers = new(StringComparer.OrdinalIgnoreCase);

    static DynamicSourceHandlerRegistry() {
        // Register built-in handlers
        RegisterHandler("const", HandleConst);
        RegisterHandler("env", HandleEnvironment);
        RegisterHandler("file", HandleFile);
    }

    /// <summary>
    /// Registers a handler for a specific source type.
    /// </summary>
    /// <param name="sourceType">The source type (e.g., "db", "api", "file")</param>
    /// <param name="handler">The handler function</param>
    public static void RegisterHandler(string sourceType, DynamicSourceHandler handler) {
        if (string.IsNullOrEmpty(sourceType)) {
            throw new ArgumentException("Source type cannot be null or empty", nameof(sourceType));
        }
        if (handler == null) {
            throw new ArgumentNullException(nameof(handler));
        }

        _handlers[sourceType] = handler;
    }

    /// <summary>
    /// Unregisters a handler for a specific source type.
    /// </summary>
    /// <param name="sourceType">The source type to unregister</param>
    /// <returns>True if the handler was removed, false if it didn't exist</returns>
    public static bool UnregisterHandler(string sourceType) {
        return _handlers.Remove(sourceType);
    }

    /// <summary>
    /// Resolves a value using the registered handler for the given source type.
    /// </summary>
    /// <param name="sourceType">The source type (e.g., "const", "env", "db")</param>
    /// <param name="sourceValue">The source-specific value</param>
    /// <param name="fallbackKey">The original key to use as fallback</param>
    /// <returns>The resolved value, or null if no handler is registered or the handler returns null</returns>
    public static string? Resolve(string sourceType, string? sourceValue, string fallbackKey) {
        if (_handlers.TryGetValue(sourceType, out var handler)) {
            return handler(sourceValue, fallbackKey);
        }
        return null; // No handler registered for this source type
    }

    /// <summary>
    /// Gets all registered source types.
    /// </summary>
    public static IReadOnlyCollection<string> RegisteredSourceTypes => _handlers.Keys;

    /// <summary>
    /// Built-in handler for 'const' source type - returns the source value as-is.
    /// </summary>
    private static string? HandleConst(string? sourceValue, string fallbackKey) {
        return sourceValue;
    }

    /// <summary>
    /// Built-in handler for 'env' source type - reads from environment variables.
    /// </summary>
    private static string? HandleEnvironment(string? sourceValue, string fallbackKey) {
        var envVarName = sourceValue ?? fallbackKey;
        return Environment.GetEnvironmentVariable(envVarName);
    }

    /// <summary>
    /// Built-in handler for 'file' source type - reads from files.
    /// </summary>
    private static string? HandleFile(string? sourceValue, string fallbackKey) {
        var filePath = sourceValue ?? fallbackKey;
        if (File.Exists(filePath)) {
            return File.ReadAllText(filePath);
        }
        return null;
    }
}
