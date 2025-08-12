using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.ProcessingInstructions;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class DocumentProcessingInstructionTests
{
    [TestMethod]
    public void DocumentPI_RoundTrip_StockProperties()
    {
        // Arrange: XferLang snippet with stock properties
    var xfer = "<! document { title \"Hello\" version \"1.2.3\" id \"abc-123\" author \"Alice\" createdAt @2025-08-12T10:15:30.0000000Z@ } !>";

        var parser = new Parser();

        // Act: parse
    var element = parser.Parse(xfer);
    var pi = element.ProcessingInstructions.OfType<DocumentProcessingInstruction>().FirstOrDefault();
        Assert.IsNotNull(pi);

        // Assert: properties present as elements
        Assert.AreEqual("Hello", pi!.Title!.Value);
        Assert.AreEqual("1.2.3", pi.Version!.Value);
        Assert.AreEqual("abc-123", pi.DocumentId!.Value);
        Assert.AreEqual("Alice", pi.Author!.Value);
        Assert.IsNotNull(pi.CreatedAt);

        // Act: serialize back
        var serialized = pi.ToXfer();
        // Assert: serializes with PI delimiters and document keyword
        StringAssert.StartsWith(serialized, "<!");
        StringAssert.Contains(serialized, "document");
        StringAssert.EndsWith(serialized, "!>");
    }

    [TestMethod]
    public void DocumentPI_RoundTrip_CustomProperties()
    {
        // Arrange: build via API with a custom property
        var pi = DocumentProcessingInstruction.Create(obj => {
            obj[DocumentProcessingInstruction.PropertyKeys.Title] = new StringElement("CfgDoc");
            obj["customFlag"] = new BooleanElement(true);
            obj["limits"] = new ObjectElement {
                ["maxItems"] = new IntegerElement(25),
                ["timeoutMs"] = new LongElement(1500)
            };
        });

        // Act: serialize and parse back
        var xfer = pi.ToXfer();
        var parser = new Parser();
    var parsed = parser.Parse(xfer).ProcessingInstructions.OfType<DocumentProcessingInstruction>().FirstOrDefault();
        Assert.IsNotNull(parsed);

        // Assert: stock + custom present
        Assert.AreEqual("CfgDoc", parsed!.Title!.Value);
        Assert.IsTrue(parsed.TryGetCustom<BooleanElement>("customFlag", out var flag) && flag!.Value);
        var limits = parsed.Get<ObjectElement>("limits");
        Assert.IsNotNull(limits);
        Assert.AreEqual(25, (limits!["maxItems"] as IntegerElement)!.Value);
        Assert.AreEqual(1500L, (limits["timeoutMs"] as LongElement)!.Value);
    }
}
