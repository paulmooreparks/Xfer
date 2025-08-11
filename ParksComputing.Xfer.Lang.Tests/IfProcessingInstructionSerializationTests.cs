using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class IfProcessingInstructionSerializationTests {
    [TestMethod]
    public void IfPI_SuccessfulCondition_StrippedFromSerialization() {
        var parser = new Parser();
        var doc = parser.Parse("[<! if eq[\"x\" \"x\"] !> \"A\"]");
        var output = doc.ToXfer();
        Assert.IsTrue(output.Contains("\"A\""));
        Assert.IsFalse(output.Contains("<!"), "Successful if PI should be stripped from serialization");
    }

    [TestMethod]
    public void IfPI_FailedCondition_StrippedAndElementSuppressed() {
        var parser = new Parser();
        var doc = parser.Parse("[<! if eq[\"x\" \"y\"] !> \"A\"]");
        var output = doc.ToXfer();
    // New policy: all if PIs are stripped from serialization regardless of outcome; failed condition suppresses element.
    Assert.IsFalse(output.Contains("\"A\""), $"Element should be suppressed when condition false. Output: {output}");
    Assert.IsFalse(output.Contains("<!"), $"If PI should be stripped even when condition false. Output: {output}");
    }

    [TestMethod]
    public void IfPI_UnknownOperator_StrippedButElementRetained() {
        var parser = new Parser();
        var doc = parser.Parse("[<! if someUnknownOp[\"a\" \"b\"] !> \"A\"]");
        var output = doc.ToXfer();
    // New policy: unknown operator treated as no-op (element included) but PI still stripped.
    Assert.IsTrue(output.Contains("\"A\""), $"Element should remain when unknown operator treated as no-op. Output: {output}");
    Assert.IsFalse(output.Contains("<!"), "Unknown operator if PI should also be stripped");
    }
}
