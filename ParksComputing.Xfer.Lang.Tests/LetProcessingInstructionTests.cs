using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class LetProcessingInstructionTests {
    [TestMethod]
    public void StandaloneLetPI_ResolvesDereference_InFollowingInterpolated() {
        var parser = new Parser();
    // Standalone let syntax uses tuple form: <! let ( name value ) !>
    var doc = parser.Parse("<! let ( greetee \"World\" ) !> 'Hello, _greetee_.'");
        // Expect root to be a tuple containing the interpolated element only (let PI suppressed)
        var output = doc.Root.ToXfer();
        Assert.IsTrue(output.Contains("Hello, World."), $"Expected interpolation to resolve greetee binding. Output: {output}");
        Assert.IsFalse(output.Contains("<!let"), "let PI should be suppressed");
    }

    [TestMethod]
    public void StandaloneLetPI_BeforeTupleChildren() {
        var parser = new Parser();
    var doc = parser.Parse("( <! let ( x 42 ) !> (_x _x) )");
        var output = doc.Root.ToXfer();
        // Expect two 42 values cloned
        Assert.IsTrue(output.Contains("(42 42)"), $"Dereferences not resolved: {output}");
    }

    [TestMethod]
    public void StandaloneLetPI_InsideTupleThenInterpolated() {
        var parser = new Parser();
    var doc = parser.Parse("( <! let ( name \"Ada\" ) !> 'Hi, _name_.' )");
        var output = doc.Root.ToXfer();
        Assert.IsTrue(output.Contains("Hi, Ada."), output);
    }
}
