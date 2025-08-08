using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.Scripting;

/// <summary>
/// Provides execution context for scripting operations including variable resolution,
/// environment access, and parser integration for expression evaluation.
/// </summary>
public class ScriptingContext {
    /// <summary>
    /// Gets or sets the dictionary of user-defined variables available for dynamic element resolution.
    /// </summary>
    public Dictionary<string, object> Variables { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the parser instance for access to parsing context and configuration.
    /// </summary>
    public Parser? Parser { get; set; }

    /// <summary>
    /// Gets or sets the current element being processed, if any.
    /// </summary>
    public Element? CurrentElement { get; set; }

    /// <summary>
    /// Gets the current platform name for platform-specific conditional logic.
    /// </summary>
    public string Platform => Environment.OSVersion.Platform.ToString();

    /// <summary>
    /// Gets the current processor architecture for architecture-specific conditional logic.
    /// </summary>
    public string Architecture => RuntimeInformation.ProcessArchitecture.ToString();

    /// <summary>
    /// Gets the current .NET runtime version.
    /// </summary>
    public Version DotNetVersion => Environment.Version;

    /// <summary>
    /// Gets a value indicating whether this is a debug build.
    /// </summary>
    public bool IsDebug =>
#if DEBUG
        true;
#else
        false;
#endif

    /// <summary>
    /// Gets the number of logical processors available on the current machine.
    /// </summary>
    public int ProcessorCount => Environment.ProcessorCount;

    /// <summary>
    /// Gets the current working directory.
    /// </summary>
    public string WorkingDirectory => Environment.CurrentDirectory;

    /// <summary>
    /// Initializes a new instance of the ScriptingContext class.
    /// </summary>
    public ScriptingContext() { }

    /// <summary>
    /// Initializes a new instance of the ScriptingContext class with the specified parser.
    /// </summary>
    /// <param name="parser">The parser instance to associate with this context.</param>
    public ScriptingContext(Parser parser) {
        Parser = parser;
    }

    /// <summary>
    /// Attempts to resolve a variable by name, checking user variables first, then built-in variables.
    /// </summary>
    /// <param name="variableName">The name of the variable to resolve.</param>
    /// <param name="value">When this method returns, contains the resolved value if found; otherwise, null.</param>
    /// <returns>True if the variable was found and resolved; otherwise, false.</returns>
    public bool TryResolveVariable(string variableName, out object? value) {
        // Check user-defined variables first
        if (Variables.TryGetValue(variableName, out value)) {
            return true;
        }

        // Check built-in variables
        value = variableName.ToUpperInvariant() switch {
            "PLATFORM" => Platform,
            "ARCHITECTURE" => Architecture,
            "DOTNET_VERSION" => DotNetVersion.ToString(),
            "PROCESSOR_COUNT" => ProcessorCount,
            "CPU_CORES" => ProcessorCount, // Alias
            "DEBUG" => IsDebug,
            "WORKING_DIRECTORY" => WorkingDirectory,
            _ => null
        };

        return value != null;
    }

    /// <summary>
    /// Gets the value of an environment variable.
    /// </summary>
    /// <param name="variableName">The name of the environment variable.</param>
    /// <returns>The value of the environment variable, or null if not found.</returns>
    public string? GetEnvironmentVariable(string variableName) {
        return Environment.GetEnvironmentVariable(variableName);
    }

    /// <summary>
    /// Sets a variable in the context.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <param name="value">The variable value.</param>
    public void SetVariable(string name, object? value) {
        if (string.IsNullOrEmpty(name)) {
            throw new ArgumentException("Variable name cannot be null or empty", nameof(name));
        }

        Variables[name] = value!;
    }

    /// <summary>
    /// Removes a variable from the context.
    /// </summary>
    /// <param name="name">The variable name to remove.</param>
    /// <returns>True if the variable was found and removed; otherwise, false.</returns>
    public bool RemoveVariable(string name) {
        if (string.IsNullOrEmpty(name)) {
            return false;
        }

        return Variables.Remove(name);
    }

    /// <summary>
    /// Clears all variables from the context (except built-in environment variables).
    /// </summary>
    public void ClearVariables() {
        Variables.Clear();
    }
}
