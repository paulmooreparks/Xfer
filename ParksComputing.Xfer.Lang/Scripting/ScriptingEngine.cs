using System;
using System.Collections.Generic;
using System.Linq;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Scripting;

/// <summary>
/// Scripting engine that provides expression evaluation capabilities using registered operators.
/// This engine coordinates the execution of scripting operators and manages their lifecycle.
/// </summary>
public class ScriptingEngine {
    private readonly Dictionary<string, ScriptingOperator> _operators;
    private readonly ScriptingContext _context;

    /// <summary>
    /// Initializes a new instance of the ScriptingEngine with a scripting context.
    /// </summary>
    /// <param name="context">The scripting context containing variables and environment information.</param>
    public ScriptingEngine(ScriptingContext context) {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _operators = new Dictionary<string, ScriptingOperator>(StringComparer.OrdinalIgnoreCase);

        RegisterBuiltInOperators();
    }

    /// <summary>
    /// Gets the scripting context used by this engine.
    /// </summary>
    public ScriptingContext Context => _context;

    /// <summary>
    /// Gets the collection of registered operator names.
    /// </summary>
    public IEnumerable<string> RegisteredOperators => _operators.Keys;

    /// <summary>
    /// Registers a scripting operator with the engine.
    /// </summary>
    /// <param name="operator">The operator to register.</param>
    /// <exception cref="ArgumentException">Thrown when an operator with the same name is already registered.</exception>
    public void RegisterOperator(ScriptingOperator @operator) {
        if (@operator == null) {
            throw new ArgumentNullException(nameof(@operator));
        }

        if (_operators.ContainsKey(@operator.OperatorName)) {
            throw new ArgumentException($"Operator '{@operator.OperatorName}' is already registered");
        }

        _operators[@operator.OperatorName] = @operator;
    }

    /// <summary>
    /// Unregisters a scripting operator from the engine.
    /// </summary>
    /// <param name="operatorName">The name of the operator to unregister.</param>
    /// <returns>True if the operator was found and removed; otherwise, false.</returns>
    public bool UnregisterOperator(string operatorName) {
        if (string.IsNullOrEmpty(operatorName)) {
            return false;
        }

        return _operators.Remove(operatorName);
    }

    /// <summary>
    /// Checks if an operator is registered with the specified name.
    /// </summary>
    /// <param name="operatorName">The name of the operator to check.</param>
    /// <returns>True if the operator is registered; otherwise, false.</returns>
    public bool IsOperatorRegistered(string operatorName) {
        return !string.IsNullOrEmpty(operatorName) && _operators.ContainsKey(operatorName);
    }

    /// <summary>
    /// Evaluates an expression using the specified operator and arguments.
    /// </summary>
    /// <param name="operatorName">The name of the operator to use.</param>
    /// <param name="arguments">The arguments to pass to the operator.</param>
    /// <returns>The result of the operator evaluation, or null if no result is available.</returns>
    /// <exception cref="ArgumentException">Thrown when the operator is not registered.</exception>
    public object? Evaluate(string operatorName, params Element[] arguments) {
        if (string.IsNullOrEmpty(operatorName)) {
            throw new ArgumentException("Operator name cannot be null or empty", nameof(operatorName));
        }

        if (!_operators.TryGetValue(operatorName, out var @operator)) {
            throw new ArgumentException($"Operator '{operatorName}' is not registered");
        }

        return @operator.Evaluate(_context, arguments ?? Array.Empty<Element>());
    }

    /// <summary>
    /// Tries to evaluate an expression using the specified operator and arguments.
    /// </summary>
    /// <param name="operatorName">The name of the operator to use.</param>
    /// <param name="arguments">The arguments to pass to the operator.</param>
    /// <param name="result">When this method returns, contains the result of the evaluation if successful.</param>
    /// <returns>True if the evaluation was successful; otherwise, false.</returns>
    public bool TryEvaluate(string operatorName, Element[] arguments, out object? result) {
        result = null;

        try {
            if (string.IsNullOrEmpty(operatorName) || !_operators.ContainsKey(operatorName)) {
                return false;
            }

            result = Evaluate(operatorName, arguments);
            return true;
        }
        catch {
            return false;
        }
    }

    /// <summary>
    /// Gets information about a registered operator.
    /// </summary>
    /// <param name="operatorName">The name of the operator.</param>
    /// <returns>The operator instance if found; otherwise, null.</returns>
    public ScriptingOperator? GetOperator(string operatorName) {
        if (string.IsNullOrEmpty(operatorName)) {
            return null;
        }

        _operators.TryGetValue(operatorName, out var @operator);
        return @operator;
    }

    /// <summary>
    /// Gets all registered operators.
    /// </summary>
    /// <returns>A collection of all registered operators.</returns>
    public IEnumerable<ScriptingOperator> GetAllOperators() {
        return _operators.Values.ToList();
    }

    /// <summary>
    /// Gets operators by category (based on namespace).
    /// </summary>
    /// <param name="category">The category to filter by (e.g., "Comparison", "Logical", "Utility").</param>
    /// <returns>A collection of operators in the specified category.</returns>
    public IEnumerable<ScriptingOperator> GetOperatorsByCategory(string category) {
        if (string.IsNullOrEmpty(category)) {
            return Enumerable.Empty<ScriptingOperator>();
        }

        return _operators.Values.Where(op =>
            op.GetType().Namespace?.EndsWith($".{category}", StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// Clears all registered operators except built-in ones.
    /// </summary>
    public void ClearCustomOperators() {
        var builtInOperators = _operators.Values
            .Where(op => op.GetType().Namespace?.Contains("ParksComputing.Xfer.Lang.Scripting") == true)
            .ToList();

        _operators.Clear();

        foreach (var builtIn in builtInOperators) {
            _operators[builtIn.OperatorName] = builtIn;
        }
    }

    /// <summary>
    /// Registers the built-in operators that come with the scripting engine.
    /// This method is called during initialization and can be called to restore built-in operators.
    /// </summary>
    private void RegisterBuiltInOperators() {
        // Ensure the global registry has been initialized
        OperatorRegistry.RegisterBuiltInOperators();

        // Register all operators from the global registry
        foreach (var operatorName in OperatorRegistry.GetRegisteredOperatorNames()) {
            var operatorInstance = OperatorRegistry.CreateOperator(operatorName);
            if (operatorInstance != null) {
                try {
                    RegisterOperator(operatorInstance);
                }
                catch (ArgumentException) {
                    // Operator already registered, skip
                }
            }
        }
    }    /// <summary>
    /// Gets diagnostic information about the scripting engine state.
    /// </summary>
    /// <returns>A dictionary containing diagnostic information.</returns>
    public Dictionary<string, object> GetDiagnosticInfo() {
        return new Dictionary<string, object> {
            ["RegisteredOperatorCount"] = _operators.Count,
            ["RegisteredOperators"] = _operators.Keys.ToList(),
            ["ContextVariableCount"] = _context.Variables.Count,
            ["ContextVariables"] = _context.Variables.Keys.ToList(),
            ["Environment"] = new Dictionary<string, object?> {
                ["Platform"] = _context.TryResolveVariable("Platform", out var platform) ? platform : "Unknown",
                ["Architecture"] = _context.TryResolveVariable("Architecture", out var arch) ? arch : "Unknown",
                ["FrameworkVersion"] = _context.TryResolveVariable("FrameworkVersion", out var fw) ? fw : "Unknown",
                ["MachineName"] = _context.TryResolveVariable("MachineName", out var machine) ? machine : "Unknown"
            }
        };
    }

    /// <summary>
    /// Returns a string representation of the scripting engine state.
    /// </summary>
    /// <returns>A string describing the engine's current state.</returns>
    public override string ToString() {
        return $"ScriptingEngine: {_operators.Count} operators registered, {_context.Variables.Count} context variables";
    }
}
