using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class ScriptProcessingInstructionWarningSuppressionTests {
    [TestMethod]
    public void LocalPass_Suppresses_UnresolvedReference_Warning_For_Later_Let() {
        var parser = new Parser();
        // The dereference of _v appears inside the target element of the script PI.
        // The let binding for v exists in the PI operations and should resolve in the local pass.
        var doc = parser.Parse("<!script ( let v 42 )!> (_v)");
        // After parse, dereference should be replaced by an IntegerElement clone.
        var serialized = doc.ToXfer();
        Assert.AreEqual("(42)", serialized, "Dereference should resolve to bound value");
        // There should be no remaining UnresolvedReference warning for 'v'.
        Assert.IsFalse(doc.Warnings.Any(w => w.Type == WarningType.UnresolvedReference && w.Context == "v"), "UnresolvedReference warning for 'v' should be suppressed");
        // There should be at least one trace indicating resolution (immediate or local-pass).
        Assert.IsTrue(doc.Warnings.Any(w => w.Type == WarningType.Trace && w.Message.Contains("resolved")), "Trace resolution message expected");
    }

    [TestMethod]
    public void Truly_Unresolved_Dereference_Retains_Warning() {
        var parser = new Parser();
        var doc = parser.Parse("(_neverBound)");
        Assert.IsTrue(doc.Warnings.Any(w => w.Type == WarningType.UnresolvedReference && w.Context == "neverBound"), "Unresolved reference should produce warning");
    }

    [TestMethod]
    public void Immediate_Resolution_Produces_Trace_No_UnresolvedReference() {
        var parser = new Parser();
        // Bind first, then dereference twice in same tuple after binding.
        var doc = parser.Parse("<!script ( let name \"value\" )!> (_name _name)");
        // Expect no unresolved reference warnings for 'name'.
        Assert.IsFalse(doc.Warnings.Any(w => w.Type == WarningType.UnresolvedReference && w.Context == "name"));
        // Expect at least one trace for deref resolved immediately and let binding trace.
        Assert.IsTrue(doc.Warnings.Count(w => w.Type == WarningType.Trace && w.Context == "name") >= 1);
    }
}
