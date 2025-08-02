using Microsoft.VisualStudio.TestTools.UnitTesting;

using ParksComputing.Xfer.Lang.ProcessingInstructions;
using ParksComputing.Xfer.Lang.Services;

namespace XferTest;

[TestClass]
public class IdProcessingInstructionTests {
    [TestMethod]
    public void InlineIdPI_AssignsIdToElement() {
        // Arrange: Xfer input with inline PI for id
        string xfer = "<! id \"myId\" !> name \"Alice\"";
        var parser = new Parser();
        var doc = parser.Parse(xfer);
        // Assert: At least one non-metadata element has the correct ID
        var ids = doc.Root.Children.Where(e => e is not ProcessingInstruction).Select(e => e.Id).ToList();
        CollectionAssert.Contains(ids, "myId");
    }

    [TestMethod]
    public void InlineIdPI_DoesNotAffectOtherElements() {
        string xfer = "<! id \"first\" !> name \"Alice\" age 42";
        var parser = new Parser();
        var doc = parser.Parse(xfer);
        var ids = doc.Root.Children.Where(e => e is not ProcessingInstruction).Select(e => e.Id).ToList();
        // Assert: Only one element has the ID "first", the other is null
        Assert.AreEqual(1, ids.Count(id => id == "first"));
        Assert.AreEqual(1, ids.Count(id => id == null));
    }

    [TestMethod]
    public void InlineIdPI_HandlesMultipleIds() {
        string xfer = "<! id \"first\" !> name \"Alice\" <! id \"second\" !> age 42";
        var parser = new Parser();
        var doc = parser.Parse(xfer);
        var ids = doc.Root.Children.Where(e => e is not ProcessingInstruction).Select(e => e.Id).ToList();
        // Assert: Both IDs are present
        CollectionAssert.Contains(ids, "first");
        CollectionAssert.Contains(ids, "second");
    }
}
