using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.Tests;

// Focused tests specifically for the original script-based let processing instruction scenario
[TestClass]
public class ScriptLetProcessingInstructionTests {
    [TestMethod]
    public void ScriptLetPI_ResolvesDereference_InFollowingInterpolated() {
        var parser = new Parser();
        var doc = parser.Parse("<! script ( let greetee \"World\" ) !> 'Hello, <_greetee_>.'");
        var output = doc.Root.ToXfer();
        Assert.IsTrue(output.Contains("Hello, World."), $"Expected interpolation to resolve greetee binding. Output: {output}");
        // Script PI containing only let should be suppressed from serialization (mirrors standalone let behavior)
        Assert.IsFalse(output.Contains("<!script"), "script PI with only let operator should be suppressed");
    }
}
