
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
        var doc = new XferDocument();
        var globalPI = new ProcessingInstructionElement(ProcessingInstructionElement.DeserializeKeyword);
        doc.MetadataCollection.Add(globalPI);
        var element = new MetadataElement();
        var resolver = new DefaultDeserializationInstructionResolver();
        var result = resolver.ResolveInstructions(element, doc);
        Assert.AreEqual(globalPI, result);
    }

    [TestMethod]
    public void ReturnsInlinePI_WhenPresent()
    {
        var doc = new XferDocument();
        var globalPI = new ProcessingInstructionElement(ProcessingInstructionElement.DeserializeKeyword);
        doc.MetadataCollection.Add(globalPI);
        var inlinePI = new ProcessingInstructionElement(ProcessingInstructionElement.DeserializeKeyword);
        var key = new IdentifierElement(ProcessingInstructionElement.DeserializeKeyword);
        var kvp = new KeyValuePairElement(key, inlinePI);
        var element = new MetadataElement();
        element.Add(kvp);
        var resolver = new DefaultDeserializationInstructionResolver();
        var result = resolver.ResolveInstructions(element, doc);
        Assert.AreEqual(inlinePI, result);
    }

    [TestMethod]
    public void ReturnsNull_WhenNoPI()
    {
        var doc = new XferDocument();
        var element = new MetadataElement();
        var resolver = new DefaultDeserializationInstructionResolver();
        var result = resolver.ResolveInstructions(element, doc);
        Assert.IsNull(result);
    }
}
