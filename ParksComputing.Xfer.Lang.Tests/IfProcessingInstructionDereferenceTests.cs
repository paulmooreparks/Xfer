using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class IfProcessingInstructionDereferenceTests {
    [TestMethod]
    public void IfPI_DereferenceBinding_False_SuppressesElement() {
        var parser = new Parser();
        // Bind showDebug to false, then test dereference in if PI
        var doc = parser.Parse("<! let ( showDebug ~false ) !> <! if _showDebug !> { debug { level \"verbose\" } }");
        var root = doc.Root.ToXfer();
        // Expect empty root collection (object removed) or absence of 'debug'
        Assert.IsFalse(root.Contains("debug"), $"Debug object should be suppressed. Output: {root}");
    }

    [TestMethod]
    public void IfPI_DereferenceBinding_True_IncludesElement() {
        var parser = new Parser();
        var doc = parser.Parse("<! let ( showDebug ~true ) !> <! if _showDebug !> { debug { level \"verbose\" } }");
        var root = doc.Root.ToXfer();
        Assert.IsTrue(root.Contains("debug"), $"Debug object should be present. Output: {root}");
    }
}
