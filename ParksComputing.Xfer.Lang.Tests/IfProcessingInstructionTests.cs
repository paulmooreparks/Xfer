using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class IfProcessingInstructionTests {
    [TestMethod]
    public void IfPI_GreaterThanOperator_SuppressesAndIncludesCorrectly() {
    // Need single root collection; embed conditional elements inside an array
    var input = "[ <! if gt[#4 #10] !> \"A\" <! if gt[#10 #4] !> \"B\" ]"; // First false (suppress), second true (include)
        var parser = new Parser();
        var doc = parser.Parse(input);
        var output = doc.ToXfer();

        Assert.IsFalse(output.Contains("A"), $"Output should not contain 'A' but was: {output}");
        Assert.IsTrue(output.Contains("B"), $"Output should contain 'B' but was: {output}");
    // TODO: Decide if successful conditional PIs should be stripped from serialization; currently they remain
    // Assert.IsFalse(output.Contains("<!"), "Processing instructions should not appear in serialized output");
    }

    [TestMethod]
    public void IfPI_UnknownOperator_NoOpDoesNotSuppress() {
    var input = "[ <! if someUnknownOp[#1 #2] !> \"X\" ]"; // Unknown operator -> no-op, element + PI retained
        var parser = new Parser();
        var doc = parser.Parse(input);
        var output = doc.ToXfer();
        Assert.IsTrue(output.Contains("X"), $"Output should contain 'X' but was: {output}");
        Assert.IsTrue(output.Contains("<!"), "Unknown operator if PI should remain serialized for visibility");
    }
}
