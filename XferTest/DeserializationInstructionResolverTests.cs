
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Deserialization;

namespace XferTest;

[TestClass]
public class DeserializationInstructionResolverTests
{
    [TestMethod]
    public void ReturnsGlobalPI_WhenNoInlinePI()
    {
        // Arrange: Add a global PI to the document
        var doc = new XferDocument();
        var globalPI = new ProcessingInstructionElement(ProcessingInstructionElement.DeserializeKeyword);
        doc.Root.Add(globalPI);
        var resolver = new DefaultDeserializationInstructionResolver();
        // Act: Try to resolve PI from a new MetadataElement
        var result = resolver.ResolveInstructions(new MetadataElement(), doc);
        // Assert: Should find a PI with correct PIType
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(ProcessingInstructionElement));
        Assert.AreEqual(ProcessingInstructionElement.DeserializeKeyword, ((ProcessingInstructionElement)result).PIType);
    }

    [TestMethod]
    public void ReturnsNull_WhenNoPI()
    {
        var doc = new XferDocument();
        var resolver = new DefaultDeserializationInstructionResolver();
        var meta = new MetadataElement();
        var result = resolver.ResolveInstructions(meta, doc);
        Assert.IsNull(result);
    }
}
