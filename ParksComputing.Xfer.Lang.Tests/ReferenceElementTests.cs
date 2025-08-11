using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class ReferenceElementTests {
    // Legacy reference semantics removed; retain a minimal regression test to ensure parser still handles backtick elements if any remain (currently none expected).

    [TestMethod]
    public void LetBinding_ReplacesSubsequentDereference() {
        var parser = new Parser();
        var doc = parser.Parse("<!script ( let x \"Hello\" )!> (_x _x)");
        var output = doc.ToXfer();
        // Depending on element spacing rules, dereferenced tuple may serialize without trailing space.
        Assert.IsTrue(output == "(\"Hello\" \"Hello\")" || output == "(\"Hello\"\"Hello\")", $"Unexpected output: {output}");
    }

    [TestMethod]
    public void StandaloneLetPI_ReplacesSubsequentDereference() {
        var parser = new Parser();
    var doc = parser.Parse("<! let ( x \"Hello\" ) !> (_x _x)");
    var outX = doc.Root.ToXfer();
    Assert.IsTrue(outX == "(\"Hello\" \"Hello\")" || outX == "(\"Hello\"\"Hello\")", $"Unexpected output: {outX}");
    }

    [TestMethod]
    public void UnresolvedDereference_WarningAndElementRetained() {
        var parser = new Parser();
        var doc = parser.Parse("(_missing)");
        var output = doc.ToXfer();
        Assert.IsTrue(output.Contains("_missing"), "Unresolved dereference should remain serialized");
        Assert.IsTrue(doc.Warnings.Exists(w => w.Type == WarningType.UnresolvedReference), "UnresolvedReference warning expected");
    }
}
