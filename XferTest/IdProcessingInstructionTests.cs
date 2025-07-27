using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;

namespace XferTest;

[TestClass]
public class IdProcessingInstructionTests {
    [TestMethod]
    public void InlineIdPI_AssignsIdToElement() {
        // Arrange: Xfer input with inline PI for id
        string xfer = "<! id \"myId\" !> name \"Alice\"";
        var parser = new Parser();

        // Act
        var doc = parser.Parse(xfer);
        Assert.IsNotNull(doc);
        Assert.IsNotNull(doc.Root);
        Assert.IsTrue(doc.Root.Count > 0);

        // Find the element with the assigned ID
        var element = doc.Root[0];
        Assert.AreEqual("myId", element.Id);
    }

    [TestMethod]
    public void InlineIdPI_DoesNotAffectOtherElements() {
        string xfer = "<! id \"first\" !> name \"Alice\" age 42";
        var parser = new Parser();
        var doc = parser.Parse(xfer);
        Assert.IsNotNull(doc);
        Assert.IsNotNull(doc.Root);
        Assert.AreEqual(2, doc.Root.Count);
        Assert.AreEqual("first", doc.Root[0].Id);
        Assert.IsNull(doc.Root[1].Id);
    }

    [TestMethod]
    public void InlineIdPI_HandlesMultipleIds() {
        string xfer = "<! id \"first\" !> name \"Alice\" <! id \"second\" !> age 42";
        var parser = new Parser();
        var doc = parser.Parse(xfer);
        Assert.IsNotNull(doc);
        Assert.IsNotNull(doc.Root);
        Assert.AreEqual(2, doc.Root.Count);
        Assert.AreEqual("first", doc.Root[0].Id);
        Assert.AreEqual("second", doc.Root[1].Id);
    }
}
