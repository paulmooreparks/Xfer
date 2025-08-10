using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Services;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class DereferenceElementTests {
    [TestMethod]
    public void Deref_Parses_As_DereferenceElement_When_Unbound() {
        var parser = new Parser();
    var doc = parser.Parse("(_foo)");
    Assert.AreEqual(1, doc.Root.Count, "Tuple should contain one element (the dereference)");
    var first = doc.Root.GetElementAt(0);
        Assert.IsInstanceOfType(first, typeof(DereferenceElement));
        var d = (DereferenceElement)first!;
        Assert.AreEqual("foo", d.Value);
    }

    [TestMethod]
    public void Identifier_With_Underscore_And_Colons_Not_Deref() {
        var parser = new Parser();
    var doc = parser.Parse("(:_foo:)");
    Assert.AreEqual(1, doc.Root.Count, "Tuple should contain one element (the identifier)");
    var first = doc.Root.GetElementAt(0);
        Assert.IsInstanceOfType(first, typeof(IdentifierElement));
        var id = (IdentifierElement)first!;
        Assert.AreEqual("_foo", id.Value);
    }

    [TestMethod]
    public void Deref_Resolves_To_Clone_When_Bound() {
        var parser = new Parser();
    var doc = parser.Parse("<!script ( let value { a 1 } )!> (_value)");
    // Root is the tuple after the script PI
    Assert.AreEqual(1, doc.Root.Count, "Dereferenced object clone expected as sole tuple element");
    var obj = doc.Root.GetElementAt(0) as ObjectElement;
        Assert.IsNotNull(obj);
        // Object should have key 'a'
        Assert.IsNotNull(obj!.GetValue("a"));
    }
}
