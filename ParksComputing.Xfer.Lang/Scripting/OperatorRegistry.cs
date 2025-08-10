using System;
using System.Collections.Generic;
using System.Linq;

namespace ParksComputing.Xfer.Lang.Scripting;

/// <summary>
/// Global registry for scripting operators that provides centralized operator management.
/// This allows operators to query for other registered operators without circular dependencies.
/// </summary>
public static class OperatorRegistry {
    private static readonly Dictionary<string, Type> _operatorTypes = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object _lock = new object();

    /// <summary>
    /// Registers an operator type with the global registry.
    /// </summary>
    /// <typeparam name="T">The operator type to register.</typeparam>
    public static void RegisterOperator<T>() where T : ScriptingOperator, new() {
        var instance = new T();
        RegisterOperator(instance.OperatorName, typeof(T));
    }

    /// <summary>
    /// Registers an operator type with the global registry by name and type.
    /// </summary>
    /// <param name="operatorName">The name of the operator.</param>
    /// <param name="operatorType">The type of the operator.</param>
    public static void RegisterOperator(string operatorName, Type operatorType) {
        if (string.IsNullOrEmpty(operatorName)) {
            throw new ArgumentException("Operator name cannot be null or empty", nameof(operatorName));
        }

        if (operatorType == null) {
            throw new ArgumentNullException(nameof(operatorType));
        }

        if (!typeof(ScriptingOperator).IsAssignableFrom(operatorType)) {
            throw new ArgumentException($"Type {operatorType.Name} must inherit from ScriptingOperator", nameof(operatorType));
        }

        lock (_lock) {
            _operatorTypes[operatorName] = operatorType;
        }
    }

    /// <summary>
    /// Checks if an operator is registered with the specified name.
    /// </summary>
    /// <param name="operatorName">The name of the operator to check.</param>
    /// <returns>True if the operator is registered; otherwise, false.</returns>
    public static bool IsOperatorRegistered(string operatorName) {
        if (string.IsNullOrEmpty(operatorName)) {
            return false;
        }

        lock (_lock) {
            return _operatorTypes.ContainsKey(operatorName);
        }
    }

    /// <summary>
    /// Gets all registered operator names.
    /// </summary>
    /// <returns>A read-only collection of registered operator names.</returns>
    public static IReadOnlyCollection<string> GetRegisteredOperatorNames() {
        lock (_lock) {
            return _operatorTypes.Keys.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Creates an instance of the specified operator.
    /// </summary>
    /// <param name="operatorName">The name of the operator to create.</param>
    /// <returns>A new instance of the operator, or null if not found.</returns>
    public static ScriptingOperator? CreateOperator(string operatorName) {
        if (string.IsNullOrEmpty(operatorName)) {
            return null;
        }

        lock (_lock) {
            if (_operatorTypes.TryGetValue(operatorName, out var operatorType)) {
                return (ScriptingOperator?)Activator.CreateInstance(operatorType);
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the type of the specified operator.
    /// </summary>
    /// <param name="operatorName">The name of the operator.</param>
    /// <returns>The type of the operator, or null if not found.</returns>
    public static Type? GetOperatorType(string operatorName) {
        if (string.IsNullOrEmpty(operatorName)) {
            return null;
        }

        lock (_lock) {
            _operatorTypes.TryGetValue(operatorName, out var operatorType);
            return operatorType;
        }
    }

    /// <summary>
    /// Unregisters an operator from the global registry.
    /// </summary>
    /// <param name="operatorName">The name of the operator to unregister.</param>
    /// <returns>True if the operator was found and removed; otherwise, false.</returns>
    public static bool UnregisterOperator(string operatorName) {
        if (string.IsNullOrEmpty(operatorName)) {
            return false;
        }

        lock (_lock) {
            return _operatorTypes.Remove(operatorName);
        }
    }

    /// <summary>
    /// Clears all registered operators.
    /// </summary>
    public static void Clear() {
        lock (_lock) {
            _operatorTypes.Clear();
        }
    }

    /// <summary>
    /// Clears all registered operators (alias for Clear method).
    /// </summary>
    public static void ClearRegistry() {
        Clear();
    }

    /// <summary>
    /// Registers all built-in operators that come with the scripting system.
    /// This method should be called during application initialization.
    /// </summary>
    public static void RegisterBuiltInOperators() {
        RegisterOperator<Utility.DefinedOperator>();
        RegisterOperator<Comparison.EqualsOperator>();
        RegisterOperator<Logical.IfOperator>();

    RegisterOperator<Comparison.GreaterThanOperator>();

    // Newly added comparison operators
    RegisterOperator<Comparison.NotEqualOperator>();
    RegisterOperator<Comparison.LessThanOperator>();
    RegisterOperator<Comparison.LessThanOrEqualOperator>();
    RegisterOperator<Comparison.GreaterThanOrEqualOperator>();

    // Newly added logical operators
    RegisterOperator<Logical.AndOperator>();
    RegisterOperator<Logical.OrOperator>();
    RegisterOperator<Logical.NotOperator>();
    RegisterOperator<Logical.XorOperator>();

    // Additional operators will be registered here as they are implemented in future phases
    }

    /// <summary>
    /// Gets diagnostic information about the operator registry.
    /// </summary>
    /// <returns>A dictionary containing diagnostic information.</returns>
    public static Dictionary<string, object> GetDiagnosticInfo() {
        lock (_lock) {
            return new Dictionary<string, object> {
                ["RegisteredOperatorCount"] = _operatorTypes.Count,
                ["RegisteredOperators"] = _operatorTypes.Keys.ToList(),
                ["OperatorTypes"] = _operatorTypes.Values.Select(t => t.Name).ToList()
            };
        }
    }
}
