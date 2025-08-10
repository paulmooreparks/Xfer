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
    Assert.IsTrue(serialized.Replace(" ", string.Empty) == "(42)", $"Dereference should resolve to bound value. Got: {serialized}");
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

    [TestMethod]
    public void ScriptPI_ObjectTarget_LetOnly_SuppressesPI_And_Resolves_Dereference() {
        var parser = new Parser();
        // PI targets a non-tuple (object) element whose value contains a dereference.
        // Dereference will be unresolved during initial parse of the object, then locally resolved and warning suppressed.
        var doc = parser.Parse("<!script ( let v 42 )!> { answer _v }");
        var xfer = doc.ToXfer();
        // PI should suppress itself (only lets) leaving only the object serialization
    Assert.IsTrue(xfer.Replace(" ", string.Empty).Contains("{answer42}"), $"Expected object with resolved value, got: {xfer}");
        // No unresolved reference warning for v should remain
        Assert.IsFalse(doc.Warnings.Any(w => w.Type == WarningType.UnresolvedReference && w.Context == "v"), "UnresolvedReference warning for 'v' should be suppressed after local-pass");
        // At least one trace should mention local-pass or binding for 'v'
        Assert.IsTrue(doc.Warnings.Any(w => w.Type == WarningType.Trace && w.Context == "v"), "Trace diagnostics for 'v' expected");
    }
}
