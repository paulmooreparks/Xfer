using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class CollectionProcessingInstructionTests {

    [TestMethod]
    public void Array_AllowsProcessingInstructions_ConditionalSuppression() {
        // First PI condition false (defined on missing var), second true
        var input = "[1 2 <! if defined <|MISSING_VAR|> !> 99 <! if defined <|PRESENT|> !> 3]";
        var parser = new Parser();
        var doc = parser.Parse(input);
        var output = doc.ToXfer();

        // 99 suppressed, 3 included
        Assert.IsFalse(output.Contains("99"), $"Expected 99 to be suppressed. Output: {output}");
        Assert.IsTrue(output.Contains("3"), $"Expected 3 to be present. Output: {output}");
    }

    [TestMethod]
    public void Object_AllowsProcessingInstructions_ConditionalSuppression() {
        // First key 'a' suppressed, second key 'b' included
        var input = "{ <! if eq[\"x\" \"y\"] !> a 1 <! if eq[\"x\" \"x\"] !> b 2 }";
        var parser = new Parser();
        var doc = parser.Parse(input);
        var output = doc.ToXfer();

        Assert.IsFalse(output.Contains("a 1"), $"Expected key 'a' to be suppressed. Output: {output}");
        Assert.IsTrue(output.Contains("b 2"), $"Expected key 'b' to be present. Output: {output}");
    }

    [TestMethod]
    public void Tuple_AllowsProcessingInstructions_ConditionalSuppression() {
        // Middle element suppressed, last included
        var input = "(1 <! if defined <|MISSING|> !> 2 <! if defined <|EXISTS|> !> 3)";
        var parser = new Parser();
        var doc = parser.Parse(input);
        var output = doc.ToXfer();

        Assert.IsTrue(output.Contains("1"), $"Tuple should contain 1. Output: {output}");
        Assert.IsFalse(output.Contains(" 2 "), $"Tuple should not contain 2. Output: {output}");
        Assert.IsTrue(output.Contains("3"), $"Tuple should contain 3. Output: {output}");
    }
}
