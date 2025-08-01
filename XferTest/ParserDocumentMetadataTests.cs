using System;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XferTest;

[TestClass]
public class ParserDocumentMetadataTests
{
    [TestMethod]
    public void ParsesDocumentLevelMetadata_WhenFirstElement()
    {
        var parser = new ParksComputing.Xfer.Lang.Services.Parser();
        string input = "<! xfer { version '1.2.3' documentVersion 'abc' custom 42 } !> foo 1";
        var doc = parser.Parse(input);
        Assert.IsNotNull(doc.Metadata);
        Assert.AreEqual("1.2.3", doc.Metadata.Xfer);
        Assert.AreEqual("abc", doc.Metadata.Version);
        Assert.IsTrue(doc.Metadata.Extensions.ContainsKey("custom"));
        var customElement = doc.Metadata.Extensions["custom"] as IntegerElement;
        Assert.IsNotNull(customElement);
        Assert.AreEqual(42, customElement.Value);
    }

    [TestMethod]
    public void ThrowsIfDocumentLevelMetadataNotFirst()
    {
        var parser = new ParksComputing.Xfer.Lang.Services.Parser();
        string input = "foo 1 <! xfer { version '1.2.3' } !> bar 2";
        var ex = Assert.ThrowsException<InvalidOperationException>(() => parser.Parse(input));
        StringAssert.Contains(ex.Message, "must be the first element");
    }

    [TestMethod]
    public void MetadataIsNotNull_WhenNotPresent()
    {
        var parser = new ParksComputing.Xfer.Lang.Services.Parser();
        string input = "foo 1 bar 2";
        var doc = parser.Parse(input);
        Assert.IsNotNull(doc.Metadata);
        Assert.IsNull(doc.Metadata.Xfer);
        Assert.IsNull(doc.Metadata.Version);
        Assert.AreEqual(0, doc.Metadata.Extensions.Count);
    }

    [TestMethod]
    public void ExtensionsContainsUnknownProperties()
    {
        var parser = new ParksComputing.Xfer.Lang.Services.Parser();
        string input = "<! xfer { foo 'bar' baz 123 } !>";
        var doc = parser.Parse(input);
        Assert.IsNotNull(doc.Metadata);
        Assert.IsNotNull(doc.Metadata.Extensions);
        Assert.IsTrue(doc.Metadata.Extensions.ContainsKey("foo"));
        Assert.IsTrue(doc.Metadata.Extensions.ContainsKey("baz"));
        Assert.IsInstanceOfType(doc.Metadata.Extensions["foo"], typeof(TextElement));
        Assert.IsInstanceOfType(doc.Metadata.Extensions["baz"], typeof(IntegerElement));
    }
}
