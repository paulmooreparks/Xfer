using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class ReferenceElementTests {
    [TestMethod]
    public void LetBinding_ReplacesSubsequentDereference() {
        var parser = new Parser();
        // Use script PI for let binding; subsequent dereference clones value
        var doc = parser.Parse("<!script ( let x \"Hello\" )!> (_x _x)");
        var output = doc.ToXfer();
        // Expect dereferences replaced, leaving tuple with two strings
        Assert.AreEqual("(\"Hello\" \"Hello\")", output);
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
