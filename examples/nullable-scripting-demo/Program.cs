using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Scripting;

namespace NullableScriptingDemo;

/// <summary>
/// Demonstrates the benefits of making ScriptingOperator.Evaluate return object? instead of object.
/// This shows how the nullable return type provides better type safety and clearer semantics.
/// </summary>
public class Program {
    public static void Main(string[] args) {
        Console.WriteLine("=== Nullable ScriptingOperator.Evaluate Demo ===");
        Console.WriteLine();
        Console.WriteLine("This demo shows the benefits of changing ScriptingOperator.Evaluate");
        Console.WriteLine("from 'object' to 'object?' return type.");
        Console.WriteLine();

        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // Set up some test variables
        context.SetVariable("definedVar", "Hello World");
        context.SetVariable("zeroValue", 0);
        context.SetVariable("emptyString", "");
        context.SetVariable("boolTrue", true);
        context.SetVariable("boolFalse", false);

        // Create test elements
        var definedElement = new DynamicElement("definedVar");
        var undefinedElement = new DynamicElement("undefinedVar");
        var trueValueElement = new StringElement("Success!");
        var falseValueElement = new StringElement("Failure!");

        DemonstrateIfOperatorNullability(engine, definedElement, undefinedElement, trueValueElement, falseValueElement);
        DemonstrateNullSafeHandling(engine, undefinedElement, trueValueElement);
        DemonstrateTypeSafety(engine, definedElement, undefinedElement, trueValueElement);
        ShowArchitecturalBenefits();

        Console.WriteLine("✅ Demo completed successfully!");
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates how the IfOperator can return null and how this is now properly typed.
    /// </summary>
    private static void DemonstrateIfOperatorNullability(ScriptingEngine engine,
        DynamicElement definedElement, DynamicElement undefinedElement,
        StringElement trueValueElement, StringElement falseValueElement) {

        Console.WriteLine("1. Testing IfOperator with nullable return:");
        Console.WriteLine("   - Condition true: returns resolved value");
        Console.WriteLine("   - Condition false with false-value: returns false-value");
        Console.WriteLine("   - Condition false without false-value: returns null");
        Console.WriteLine();

        // Test case 1: Condition true - should return "Success!"
        var result1 = engine.Evaluate("if", definedElement, trueValueElement);
        Console.WriteLine($"✓ if(definedVar, 'Success!'): {result1 ?? "null"} (Type: {result1?.GetType().Name ?? "null"})");

        // Test case 2: Condition false with false-value - should return "Failure!"
        var result2 = engine.Evaluate("if", undefinedElement, trueValueElement, falseValueElement);
        Console.WriteLine($"✓ if(undefinedVar, 'Success!', 'Failure!'): {result2 ?? "null"} (Type: {result2?.GetType().Name ?? "null"})");

        // Test case 3: Condition false without false-value - should return null
        var result3 = engine.Evaluate("if", undefinedElement, trueValueElement);
        Console.WriteLine($"✓ if(undefinedVar, 'Success!'): {result3 ?? "null"} (Type: {result3?.GetType().Name ?? "null"})");

        // Test case 4: Let's also test with a variable that resolves to false
        var zeroElement = new DynamicElement("zeroValue");
        var result4 = engine.Evaluate("if", zeroElement, trueValueElement);
        Console.WriteLine($"✓ if(zeroValue=0, 'Success!'): {result4 ?? "null"} (Type: {result4?.GetType().Name ?? "null"})");

        // Test case 5: Test with empty string (should be falsy)
        var emptyElement = new DynamicElement("emptyString");
        var result5 = engine.Evaluate("if", emptyElement, trueValueElement);
        Console.WriteLine($"✓ if(emptyString='', 'Success!'): {result5 ?? "null"} (Type: {result5?.GetType().Name ?? "null"})");

        // Test case 6: Test with explicit false boolean
        var falseElement = new DynamicElement("boolFalse");
        var result6 = engine.Evaluate("if", falseElement, trueValueElement);
        Console.WriteLine($"✓ if(boolFalse=false, 'Success!'): {result6 ?? "null"} (Type: {result6?.GetType().Name ?? "null"})");

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates various null-safe handling patterns enabled by the nullable return type.
    /// </summary>
    private static void DemonstrateNullSafeHandling(ScriptingEngine engine,
        DynamicElement undefinedElement, StringElement trueValueElement) {

        Console.WriteLine("2. Demonstrating null-safe handling patterns:");
        Console.WriteLine("   - Using null-conditional operators (?.)");
        Console.WriteLine("   - Using null-coalescing operators (??)");
        Console.WriteLine("   - Using pattern matching (is/switch)");
        Console.WriteLine();

        // Get a result that we know will be null (false condition with no false-value)
        var falseElement = new DynamicElement("boolFalse");
        var nullableResult = engine.Evaluate("if", falseElement, trueValueElement);

        // Pattern 1: Null-conditional operator
        var stringLength = nullableResult?.ToString()?.Length ?? 0;
        Console.WriteLine($"✓ Length using ?.: {stringLength}");

        // Pattern 2: Null-coalescing operator
        var safeValue = nullableResult ?? "DEFAULT_VALUE";
        Console.WriteLine($"✓ Value using ??: {safeValue}");

        // Pattern 3: Pattern matching with is
        if (nullableResult is string str) {
            Console.WriteLine($"✓ Got string: {str}");
        } else {
            Console.WriteLine($"✓ Got non-string or null: {nullableResult?.GetType().Name ?? "null"}");
        }

        // Pattern 4: Switch expression
        var message = nullableResult switch {
            string s => $"String value: {s}",
            null => "Null result (no false-value provided)",
            bool b => $"Boolean value: {b}",
            _ => $"Other type: {nullableResult.GetType().Name} = {nullableResult}"
        };
        Console.WriteLine($"✓ Switch expression: {message}");

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates the type safety benefits of the nullable return type.
    /// </summary>
    private static void DemonstrateTypeSafety(ScriptingEngine engine,
        DynamicElement definedElement, DynamicElement undefinedElement, StringElement trueValueElement) {

        Console.WriteLine("3. Type safety and IntelliSense benefits:");
        Console.WriteLine("   - Compiler warnings for potential null usage");
        Console.WriteLine("   - Better IDE support with nullable annotations");
        Console.WriteLine("   - Forced handling of null cases");
        Console.WriteLine();

        // This would now generate a compiler warning if we tried to use the result
        // without checking for null first
        var falseElement = new DynamicElement("boolFalse");
        var result = engine.Evaluate("if", falseElement, trueValueElement);

        // Safe usage patterns
        if (result != null) {
            Console.WriteLine($"✓ Safe usage: {result}");
        } else {
            Console.WriteLine("✓ Null case handled properly");
        }

        // Compare different operator results
        var definedResult = engine.Evaluate("defined", definedElement);
        var equalsResult = engine.Evaluate("eq", definedElement, new StringElement("Hello World"));

        Console.WriteLine($"✓ defined(definedVar): {definedResult} (Type: {definedResult?.GetType().Name ?? "null"})");
        Console.WriteLine($"✓ eq(definedVar, 'Hello World'): {equalsResult} (Type: {equalsResult?.GetType().Name ?? "null"})");

        Console.WriteLine();
    }

    /// <summary>
    /// Shows the architectural and development benefits of the nullable return type.
    /// </summary>
    private static void ShowArchitecturalBenefits() {
        Console.WriteLine("4. Architectural and development benefits:");
        Console.WriteLine();

        Console.WriteLine("✅ Honest API design:");
        Console.WriteLine("   Before: object Evaluate(...) // Could return null but didn't indicate it");
        Console.WriteLine("   After:  object? Evaluate(...) // Clearly indicates null is a valid return");
        Console.WriteLine();

        Console.WriteLine("✅ Better developer experience:");
        Console.WriteLine("   - IntelliSense shows nullable warnings");
        Console.WriteLine("   - Compiler helps catch potential null reference exceptions");
        Console.WriteLine("   - Code is more self-documenting about null possibilities");
        Console.WriteLine();

        Console.WriteLine("✅ Semantic clarity:");
        Console.WriteLine("   - null result means 'no value available' for conditional logic");
        Console.WriteLine("   - Consistent with C# nullable reference types");
        Console.WriteLine("   - Matches the actual behavior of operators like IfOperator");
        Console.WriteLine();

        Console.WriteLine("✅ Code quality improvements:");
        Console.WriteLine("   - No more pragma warnings needed in operator implementations");
        Console.WriteLine("   - Forces callers to handle null cases explicitly");
        Console.WriteLine("   - Reduces runtime null reference exceptions");
        Console.WriteLine();

        Console.WriteLine("✅ Backward compatibility:");
        Console.WriteLine("   - Existing code continues to work");
        Console.WriteLine("   - Only compilation warnings for potentially unsafe usage");
        Console.WriteLine("   - Easy migration path with null-conditional operators");
        Console.WriteLine();
    }
}
