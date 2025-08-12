using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class ParserFragmentTests
{
    [TestMethod]
    public void ParseFragment_String_ReturnsStringElement()
    {
        var parser = new Parser();
        var elem = parser.ParseFragment("<\"hello\">");
        Assert.IsInstanceOfType(elem, typeof(StringElement));
        Assert.AreEqual("hello", ((StringElement)elem).Value);
    }

    [TestMethod]
    public void ParseFragmentMany_TwoStrings_ReturnsTupleWithTwoChildren()
    {
        var parser = new Parser();
        var tuple = parser.ParseFragmentMany("<\"a\"> <\"b\">");
        Assert.AreEqual(2, tuple.Children.Count);
        Assert.AreEqual("a", ((StringElement)tuple.Children[0]).Value);
        Assert.AreEqual("b", ((StringElement)tuple.Children[1]).Value);
    }

    [TestMethod]
    public void ParseFragment_TrailingContent_Throws()
    {
        var parser = new Parser();
        Assert.ThrowsException<InvalidOperationException>(() => parser.ParseFragment("<\"a\"> junk"));
    }
}
