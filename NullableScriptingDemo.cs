using System;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Scripting;

/// <summary>
/// Demonstrates the benefits of making ScriptingOperator.Evaluate return object? instead of object.
/// This shows how the nullable return type provides better type safety and clearer semantics.
/// </summary>
public class NullableScriptingDemo {
    public static void Main() {
        Console.WriteLine("=== Nullable ScriptingOperator.Evaluate Demo ===");
        Console.WriteLine();

        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // Set up some test variables
        context.SetVariable("definedVar", "Hello World");
        context.SetVariable("zeroValue", 0);
        context.SetVariable("emptyString", "");

        // Create test elements
        var definedElement = new DynamicElement("definedVar");
        var undefinedElement = new DynamicElement("undefinedVar");
        var trueValueElement = new QuotedElement("Success!");
        var falseValueElement = new QuotedElement("Failure!");

        Console.WriteLine("1. Testing IfOperator with nullable return:");
        Console.WriteLine("   - Condition true: returns resolved value");
        Console.WriteLine("   - Condition false with false-value: returns false-value");
        Console.WriteLine("   - Condition false without false-value: returns null");
        Console.WriteLine();

        // Test case 1: Condition true - should return "Success!"
        var result1 = engine.Evaluate("if", definedElement, trueValueElement);
        Console.WriteLine($"if(definedVar, 'Success!'): {result1 ?? "null"} (Type: {result1?.GetType().Name ?? "null"})");

        // Test case 2: Condition false with false-value - should return "Failure!"
        var result2 = engine.Evaluate("if", undefinedElement, trueValueElement, falseValueElement);
        Console.WriteLine($"if(undefinedVar, 'Success!', 'Failure!'): {result2 ?? "null"} (Type: {result2?.GetType().Name ?? "null"})");

        // Test case 3: Condition false without false-value - should return null
        var result3 = engine.Evaluate("if", undefinedElement, trueValueElement);
        Console.WriteLine($"if(undefinedVar, 'Success!'): {result3 ?? "null"} (Type: {result3?.GetType().Name ?? "null"})");

        Console.WriteLine();
        Console.WriteLine("2. Demonstrating null-safe handling:");
        Console.WriteLine("   - Using null-conditional operators (?.)");
        Console.WriteLine("   - Using null-coalescing operators (??)");
        Console.WriteLine("   - Using pattern matching (is)");
        Console.WriteLine();

        // Demonstrate null-safe handling patterns
        var nullableResult = engine.Evaluate("if", undefinedElement, trueValueElement);

        // Pattern 1: Null-conditional operator
        var stringLength = nullableResult?.ToString()?.Length ?? 0;
        Console.WriteLine($"Length using ?.: {stringLength}");

        // Pattern 2: Null-coalescing operator
        var safeValue = nullableResult ?? "DEFAULT";
        Console.WriteLine($"Value using ??: {safeValue}");

        // Pattern 3: Pattern matching
        var message = nullableResult switch {
            string str => $"Got string: {str}",
            null => "Got null result",
            _ => $"Got {nullableResult.GetType().Name}: {nullableResult}"
        };
        Console.WriteLine($"Pattern matching: {message}");

        Console.WriteLine();
        Console.WriteLine("3. Benefits of nullable return type:");
        Console.WriteLine("   ✅ Honest API - accurately reflects what operators return");
        Console.WriteLine("   ✅ Better IntelliSense - IDE shows nullable warnings");
        Console.WriteLine("   ✅ Semantic clarity - null means 'no value' for conditionals");
        Console.WriteLine("   ✅ Type safety - forces developers to handle null cases");
        Console.WriteLine("   ✅ No more pragma warnings needed in operators");
        Console.WriteLine();

        Console.WriteLine("4. Comparison with before/after:");
        Console.WriteLine("   Before: object Evaluate(...) // Could return null but didn't say so");
        Console.WriteLine("   After:  object? Evaluate(...) // Clearly indicates null is possible");
        Console.WriteLine();
        Console.WriteLine("Demo completed successfully!");
    }
}
