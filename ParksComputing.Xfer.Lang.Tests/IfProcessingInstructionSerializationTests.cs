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
    public void IfPI_FailedCondition_RemainsForVisibility() {
        var parser = new Parser();
        var doc = parser.Parse("[<! if eq[\"x\" \"y\"] !> \"A\"]");
        var output = doc.ToXfer();
    Assert.IsFalse(output.Contains("\"A\""));
    Assert.IsTrue(output.Contains("<!"), $"Failed condition PI should remain for visibility. Output: {output}");
    }

    [TestMethod]
    public void IfPI_UnknownOperator_RemainsForVisibility() {
        var parser = new Parser();
        var doc = parser.Parse("[<! if someUnknownOp[\"a\" \"b\"] !> \"A\"]");
        var output = doc.ToXfer();
        Assert.IsTrue(output.Contains("\"A\""));
        Assert.IsTrue(output.Contains("<!"), "Unknown operator PI should remain");
    }
}
