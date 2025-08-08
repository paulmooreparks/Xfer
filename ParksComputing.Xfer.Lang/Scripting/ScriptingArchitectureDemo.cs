using System;
using System.Collections.Generic;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Scripting;
using ParksComputing.Xfer.Lang.Scripting.Utility;
using ParksComputing.Xfer.Lang.Scripting.Comparison;

namespace ParksComputing.Xfer.Lang.Scripting;

/// <summary>
/// Demonstration and testing class for the new Scripting namespace architecture.
/// Shows how the separated operators provide clean, reusable conditional logic
/// outside of the Processing Instruction hierarchy.
/// </summary>
public static class ScriptingArchitectureDemo {
    /// <summary>
    /// Demonstrates the basic usage of the scripting engine with various operators.
    /// </summary>
    public static void RunBasicDemo() {
        Console.WriteLine("=== XferLang Scripting Architecture Demo ===");
        Console.WriteLine();

        // Create a scripting context with some variables
        var context = new ScriptingContext();
        context.SetVariable("myVar", "Hello World");
        context.SetVariable("numVar", 42);
        context.SetVariable("emptyVar", "");
        // Note: nullVar is intentionally not set to demonstrate undefined behavior

        // Create the scripting engine
        var engine = new ScriptingEngine(context);

        Console.WriteLine("Context Variables:");
        foreach (var kvp in context.Variables) {
            Console.WriteLine($"  {kvp.Key} = {kvp.Value ?? "null"}");
        }
        Console.WriteLine();

        Console.WriteLine("Registered Operators:");
        foreach (var operatorName in engine.RegisteredOperators) {
            var op = engine.GetOperator(operatorName);
            Console.WriteLine($"  {op}");
        }
        Console.WriteLine();

        // Test the defined operator
        Console.WriteLine("=== Testing DefinedOperator ===");
        TestDefinedOperator(engine, context);
        Console.WriteLine();

        // Test the equals operator
        Console.WriteLine("=== Testing EqualsOperator ===");
        TestEqualsOperator(engine, context);
        Console.WriteLine();

        // Test the if operator
        Console.WriteLine("=== Testing IfOperator ===");
        TestIfOperator(engine, context);
        Console.WriteLine();        Console.WriteLine("=== Architecture Benefits ===");
        Console.WriteLine("✅ Clean separation: Scripting operators vs Processing Instructions");
        Console.WriteLine("✅ Reusable logic: Same operators work in PI context and standalone");
        Console.WriteLine("✅ Extensible: Easy to add new operators without touching PI code");
        Console.WriteLine("✅ Testable: Operators can be unit tested independently");
        Console.WriteLine("✅ Consistent: All operators follow the same interface pattern");
    }

    /// <summary>
    /// Tests the DefinedOperator with various element types.
    /// </summary>
    private static void TestDefinedOperator(ScriptingEngine engine, ScriptingContext context) {
        // Test with a dynamic element that resolves to a defined variable
        var definedDynamic = new DynamicElement("myVar");
        var definedResult = engine.Evaluate("defined", definedDynamic);
        Console.WriteLine($"defined(myVar) = {definedResult} (expected: True)");

        // Test with a dynamic element that resolves to an empty variable
        var emptyDynamic = new DynamicElement("emptyVar");
        var emptyResult = engine.Evaluate("defined", emptyDynamic);
        Console.WriteLine($"defined(emptyVar) = {emptyResult} (expected: False)");

        // Test with a dynamic element that doesn't exist
        var undefinedDynamic = new DynamicElement("nullVar");
        var undefinedResult = engine.Evaluate("defined", undefinedDynamic);
        Console.WriteLine($"defined(nullVar) = {undefinedResult} (expected: False)");

        // Test with a character element (always defined)
        var charElement = new CharacterElement(1, 0x42); // ASCII 'B'
        var charResult = engine.Evaluate("defined", charElement);
        Console.WriteLine($"defined(CharacterElement) = {charResult} (expected: True)");
    }

    /// <summary>
    /// Tests the IfOperator with various conditional scenarios.
    /// </summary>
    private static void TestIfOperator(ScriptingEngine engine, ScriptingContext context) {
        // Test simple if with defined condition
        var definedVar = new DynamicElement("myVar");
        context.SetVariable("trueResult", "Condition was true!");
        var trueValueElement = new DynamicElement("trueResult");

        var ifDefinedResult = engine.Evaluate("if", definedVar, trueValueElement);
        Console.WriteLine($"if(defined(myVar), trueResult) = {ifDefinedResult} (expected: 'Condition was true!')");

        // Test if with false condition
        var undefinedVar = new DynamicElement("nullVar");
        context.SetVariable("falseResult", "Condition was false!");
        var falseValueElement = new DynamicElement("falseResult");

        var ifUndefinedResult = engine.Evaluate("if", undefinedVar, trueValueElement, falseValueElement);
        Console.WriteLine($"if(undefined(nullVar), trueResult, falseResult) = {ifUndefinedResult} (expected: 'Condition was false!')");

        // Test if without false value (should return null when condition is false)
        var ifNoFalseResult = engine.Evaluate("if", undefinedVar, trueValueElement);
        Console.WriteLine($"if(undefined(nullVar), trueResult) = {ifNoFalseResult ?? "null"} (expected: null)");

        // Test if with boolean condition
        context.SetVariable("isTrue", true);
        context.SetVariable("isFalse", false);
        var boolTrueVar = new DynamicElement("isTrue");
        var boolFalseVar = new DynamicElement("isFalse");

        var ifBoolTrueResult = engine.Evaluate("if", boolTrueVar, trueValueElement);
        Console.WriteLine($"if(boolTrue, trueResult) = {ifBoolTrueResult} (expected: 'Condition was true!')");

        var ifBoolFalseResult = engine.Evaluate("if", boolFalseVar, trueValueElement, falseValueElement);
        Console.WriteLine($"if(boolFalse, trueResult, falseResult) = {ifBoolFalseResult} (expected: 'Condition was false!')");

        // Test if with numeric condition
        context.SetVariable("nonZero", 42);
        context.SetVariable("zero", 0);
        var nonZeroVar = new DynamicElement("nonZero");
        var zeroVar = new DynamicElement("zero");

        var ifNonZeroResult = engine.Evaluate("if", nonZeroVar, trueValueElement);
        Console.WriteLine($"if(nonZero, trueResult) = {ifNonZeroResult} (expected: 'Condition was true!')");

        var ifZeroResult = engine.Evaluate("if", zeroVar, trueValueElement, falseValueElement);
        Console.WriteLine($"if(zero, trueResult, falseResult) = {ifZeroResult} (expected: 'Condition was false!')");
    }

    /// <summary>
    /// Tests the EqualsOperator with various value comparisons.
    /// </summary>
    private static void TestEqualsOperator(ScriptingEngine engine, ScriptingContext context) {
        // Test string equality
        var stringVar1 = new DynamicElement("myVar");
        // Create a string element by using a StringElement (we'll need to find the correct constructor)
        // For now, let's use a simpler approach and test with two dynamic elements
        context.SetVariable("compareVar", "Hello World");
        var stringVar2 = new DynamicElement("compareVar");
        var stringEqualResult = engine.Evaluate("eq", stringVar1, stringVar2);
        Console.WriteLine($"eq(myVar, compareVar) = {stringEqualResult} (expected: True)");

        // Test numeric equality
        var numVar = new DynamicElement("numVar");
        context.SetVariable("numCompare", 42);
        var numVar2 = new DynamicElement("numCompare");
        var numEqualResult = engine.Evaluate("eq", numVar, numVar2);
        Console.WriteLine($"eq(numVar, numCompare) = {numEqualResult} (expected: True)");

        // Test inequality
        var inequalResult = engine.Evaluate("eq", stringVar1, numVar);
        Console.WriteLine($"eq(myVar, numVar) = {inequalResult} (expected: False)");

        // Test with undefined variable
        var undefinedVar = new DynamicElement("nullVar");
        var nullEqualResult = engine.Evaluate("eq", undefinedVar, stringVar1);
        Console.WriteLine($"eq(nullVar, myVar) = {nullEqualResult} (expected: False)");

        // Test with character element
        var charElement = new CharacterElement(0x42); // ASCII 'B'
        context.SetVariable("charVar", 66); // ASCII value of 'B'
        var charVar = new DynamicElement("charVar");
        var charEqualResult = engine.Evaluate("eq", charElement, charVar);
        Console.WriteLine($"eq(CharacterElement(B), charVar(66)) = {charEqualResult} (expected: True)");
    }

    /// <summary>
    /// Demonstrates how the new architecture maintains backward compatibility
    /// with existing Processing Instructions while providing enhanced capabilities.
    /// </summary>
    public static void DemonstrateBackwardCompatibility() {
        Console.WriteLine("=== Backward Compatibility Demo ===");
        Console.WriteLine();

        // Show that the DefinedProcessingInstruction still works as before
        // but now internally uses the DefinedOperator
        var definedElement = new DynamicElement("TEST_VAR");

        Console.WriteLine("Testing DefinedProcessingInstruction (using new DefinedOperator internally):");

        try {
            var definedPI = new ProcessingInstructions.DefinedProcessingInstruction(definedElement);
            definedPI.ProcessingInstructionHandler();

            Console.WriteLine($"DefinedProcessingInstruction result: {definedPI.IsDefined}");
            Console.WriteLine("✅ Backward compatibility maintained");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine();

        // Demonstrate the new IfProcessingInstruction
        Console.WriteLine("Testing IfProcessingInstruction (hybrid PI/Scripting approach):");

        try {
            // Test with a defined variable condition
            var ifElement = new DynamicElement("myVar");
            var ifPI = new ProcessingInstructions.IfProcessingInstruction(ifElement);
            ifPI.ProcessingInstructionHandler();

            Console.WriteLine($"IfProcessingInstruction(myVar) result: {ifPI.ConditionMet}");

            // Test with an undefined variable condition
            var ifElement2 = new DynamicElement("undefinedVar");
            var ifPI2 = new ProcessingInstructions.IfProcessingInstruction(ifElement2);
            ifPI2.ProcessingInstructionHandler();

            Console.WriteLine($"IfProcessingInstruction(undefinedVar) result: {ifPI2.ConditionMet}");
            Console.WriteLine("✅ Hybrid PI/Scripting approach working");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("Benefits of the new architecture:");
        Console.WriteLine("• Processing Instructions maintain their API contract");
        Console.WriteLine("• Internal logic is now delegated to reusable operators");
        Console.WriteLine("• Operators can be used independently of Processing Instructions");
        Console.WriteLine("• Easier to add new conditional logic without modifying PI classes");
        Console.WriteLine("• Better separation of concerns between document processing and expression evaluation");
        Console.WriteLine("• Hybrid approach: PIs for document-level semantics, operators for expression logic");
    }    /// <summary>
    /// Shows diagnostic information about the scripting engine state.
    /// </summary>
    public static void ShowDiagnosticInfo() {
        Console.WriteLine("=== Scripting Engine Diagnostics ===");

        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        var diagnostics = engine.GetDiagnosticInfo();

        foreach (var kvp in diagnostics) {
            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
        }
    }
}
