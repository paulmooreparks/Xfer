using System;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Scripting.Utility;

/// <summary>
/// Scripting operator that checks if an element has a meaningful value (is "defined").
/// This is the extracted core logic from DefinedProcessingInstruction, designed to be
/// reusable across different contexts within the scripting system.
/// </summary>
public class DefinedOperator : ScriptingOperator {
    /// <summary>
    /// Gets the unique name identifier for this operator.
    /// </summary>
    public override string OperatorName => "defined";

    /// <summary>
    /// Gets a brief description of what this operator does.
    /// </summary>
    public override string Description => "Checks if an element has a meaningful value (is defined)";

    /// <summary>
    /// Gets the minimum number of arguments this operator requires.
    /// The defined operator requires exactly one argument to check.
    /// </summary>
    public override int MinArguments => 1;

    /// <summary>
    /// Gets the maximum number of arguments this operator accepts.
    /// The defined operator accepts exactly one argument.
    /// </summary>
    public override int MaxArguments => 1;

    /// <summary>
    /// Evaluates whether the specified element is "defined" (has a meaningful value).
    ///
    /// Definition semantics:
    /// - An element is considered "defined" if its ParsedValue is not null
    /// - For DynamicElement: Uses runtime value resolution with string.IsNullOrEmpty check
    /// - For CollectionElement: Defined if has at least one child element
    /// - For other elements: Defined if ParsedValue is not null
    /// - Variables from context: Defined if variable exists and has non-null value
    /// </summary>
    /// <param name="context">The scripting context containing variables and environment information.</param>
    /// <param name="arguments">The element to check for definition. Must contain exactly one element.</param>
    /// <returns>Boolean true if the element is defined; otherwise, false.</returns>
    public override object? Evaluate(ScriptingContext context, params Element[] arguments) {
        ValidateArguments(arguments);

        var element = arguments[0];

        // Handle null elements
        if (element == null) {
            return false;
        }

        // Use the element's ParsedValue to determine if it's defined
        // This leverages each element type's own logic for what constitutes a "value"
        var parsedValue = element.ParsedValue;

        // Special handling for dynamic elements - check if we can resolve from context
        if (element is DynamicElement dynamicElement) {
            return EvaluateDynamicElementDefined(dynamicElement, context);
        }

        // For all other elements, they are defined if they have a non-null ParsedValue
        return parsedValue != null;
    }

    /// <summary>
    /// Evaluates whether a dynamic element is defined, considering both its resolved value
    /// and potential variable lookups in the scripting context.
    /// </summary>
    /// <param name="dynamicElement">The dynamic element to evaluate.</param>
    /// <param name="context">The scripting context for variable resolution.</param>
    /// <returns>True if the dynamic element is defined; otherwise, false.</returns>
    private bool EvaluateDynamicElementDefined(DynamicElement dynamicElement, ScriptingContext context) {
        // First, check if the dynamic element itself has a resolved value
        // DynamicElement.ParsedValue uses string.IsNullOrEmpty, so null means "not defined"
        var dynamicValue = dynamicElement.ParsedValue;
        if (dynamicValue != null) {
            return true;
        }

        // If the dynamic element doesn't have a resolved value, check if we can resolve
        // it as a variable from the scripting context
        var variableName = dynamicElement.Value; // The raw variable name
        if (context.TryResolveVariable(variableName, out var contextValue)) {
            // Variable exists in context - it's defined if the value is not null
            return contextValue != null;
        }

        // Neither the dynamic element nor context variable resolution found a value
        return false;
    }

    /// <summary>
    /// Provides additional validation specific to the defined operator.
    /// Ensures the argument is a valid element that can be checked for definition.
    /// </summary>
    /// <param name="arguments">The arguments to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the argument is not a valid element.</exception>
    protected override void ValidateArguments(Element[] arguments) {
        base.ValidateArguments(arguments);

        if (arguments[0] == null) {
            throw new ArgumentException("The 'defined' operator requires a non-null element to evaluate");
        }

        // The defined operator can work with any element type, so no additional type validation needed
    }

    /// <summary>
    /// Returns a detailed string representation of this operator.
    /// </summary>
    /// <returns>A string describing the operator and its usage.</returns>
    public override string ToString() {
        return $"{OperatorName}: {Description} - Usage: defined(element)";
    }
}
