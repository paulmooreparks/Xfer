using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Tests;

/// <summary>
/// Tests for DOM-like query and navigation functionality in XferLang.
/// Tests both Element-level and Document-level query methods, as well as tree navigation.
/// </summary>
[TestClass]
public class QueryNavigationTests {

    /// <summary>
    /// Creates a test document with a complex structure for query and navigation testing.
    /// Structure:
    /// Root (TupleElement)
    ///   - Header (ObjectElement, id="header", tag="section")
    ///     - Title (StringElement, id="title", tag="text")
    ///     - Subtitle (StringElement, tag="text")
    ///   - Content (TupleElement, id="content", tag="section")
    ///     - Item1 (StringElement, tag="item")
    ///     - Item2 (IntegerElement, tag="item")
    ///     - Item3 (BooleanElement, tag="item")
    ///   - Footer (ObjectElement, id="footer", tag="section")
    ///     - Copyright (StringElement, tag="text")
    /// </summary>
    private XferDocument CreateTestDocument() {
        var doc = new XferDocument();

        // Create elements
        var root = new TupleElement();
        doc.Root = root;

        // Header section
        var header = new ObjectElement { Id = "header", Tag = "section" };
        var title = new StringElement("Test Document") { Id = "title", Tag = "text" };
        var subtitle = new StringElement("Subtitle") { Tag = "text" };

        header.AddOrUpdate(new KeyValuePairElement(new IdentifierElement("title"), title));
        header.AddOrUpdate(new KeyValuePairElement(new IdentifierElement("subtitle"), subtitle));
        root.Add(header);

        // Content section
        var content = new TupleElement { Id = "content", Tag = "section" };
        var item1 = new StringElement("First Item") { Tag = "item" };
        var item2 = new IntegerElement(42) { Tag = "item" };
        var item3 = new BooleanElement(true) { Tag = "item" };

        content.Add(item1);
        content.Add(item2);
        content.Add(item3);
        root.Add(content);

        // Footer section
        var footer = new ObjectElement { Id = "footer", Tag = "section" };
        var copyright = new StringElement("© 2023") { Tag = "text" };

        footer.AddOrUpdate(new KeyValuePairElement(new IdentifierElement("copyright"), copyright));
        root.Add(footer);

        return doc;
    }

    #region Document-Level Query Tests

    [TestMethod]
    public void GetElementsByType_Generic_ReturnsCorrectElements() {
        // Arrange
        var doc = CreateTestDocument();

        // Act
        var stringElements = doc.GetElementsByType<StringElement>();

        // Assert
        Assert.AreEqual(4, stringElements.Count); // title, subtitle, item1, copyright
        Assert.IsTrue(stringElements.Any(e => e.Id == "title"));
        Assert.IsTrue(stringElements.Any(e => e.Tag == "text"));
    }

    [TestMethod]
    public void GetElementsByType_NonGeneric_ReturnsCorrectElements() {
        // Arrange
        var doc = CreateTestDocument();

        // Act
        var objectElements = doc.GetElementsByType(typeof(ObjectElement));

        // Assert
        Assert.AreEqual(2, objectElements.Count); // header and footer
        Assert.IsTrue(objectElements.Any(e => e.Id == "header"));
        Assert.IsTrue(objectElements.Any(e => e.Id == "footer"));
    }

    #endregion

    #region Element-Level Query Tests

    [TestMethod]
    public void Element_FindElementsByTag_ReturnsCorrectElements() {
        // Arrange
        var doc = CreateTestDocument();

        // Act
        var sectionElements = doc.Root.FindElementsByTag("section");
        var textElements = doc.Root.FindElementsByTag("text");
        var itemElements = doc.Root.FindElementsByTag("item");

        // Assert
        Assert.AreEqual(3, sectionElements.Count); // header, content, and footer
        Assert.AreEqual(3, textElements.Count); // title, subtitle, copyright
        Assert.AreEqual(3, itemElements.Count); // item1, item2, item3
    }

    [TestMethod]
    public void Element_FindElementsByType_ReturnsCorrectElements() {
        // Arrange
        var doc = CreateTestDocument();

        // Act
        var stringElements = doc.Root.FindElementsByType<StringElement>();
        var integerElements = doc.Root.FindElementsByType<IntegerElement>();
        var booleanElements = doc.Root.FindElementsByType<BooleanElement>();

        // Assert
        Assert.AreEqual(4, stringElements.Count); // title, subtitle, item1, copyright
        Assert.AreEqual(1, integerElements.Count); // item2
        Assert.AreEqual(1, booleanElements.Count); // item3
    }

    #endregion

    #region Navigation Tests

    [TestMethod]
    public void NextSibling_ReturnsCorrectElement() {
        // Arrange
        var doc = CreateTestDocument();
        var header = doc.Root.FindElementById("header");
        var content = doc.Root.FindElementById("content");

        // Act
        var nextSibling = header?.NextSibling;

        // Assert
        Assert.IsNotNull(nextSibling);
        Assert.AreEqual(content, nextSibling);
        Assert.AreEqual("content", nextSibling.Id);
    }

    [TestMethod]
    public void PreviousSibling_ReturnsCorrectElement() {
        // Arrange
        var doc = CreateTestDocument();
        var content = doc.Root.FindElementById("content");
        var header = doc.Root.FindElementById("header");

        // Act
        var previousSibling = content?.PreviousSibling;

        // Assert
        Assert.IsNotNull(previousSibling);
        Assert.AreEqual(header, previousSibling);
        Assert.AreEqual("header", previousSibling.Id);
    }

    [TestMethod]
    public void FirstChild_LastChild_ReturnCorrectElements() {
        // Arrange
        var doc = CreateTestDocument();
        var root = doc.Root;

        // Act
        var firstChild = root.FirstChild;
        var lastChild = root.LastChild;

        // Assert
        Assert.IsNotNull(firstChild);
        Assert.IsNotNull(lastChild);
        Assert.AreEqual("header", firstChild.Id);
        Assert.AreEqual("footer", lastChild.Id);
    }

    [TestMethod]
    public void FirstChild_LastChild_EmptyElement_ReturnsNull() {
        // Arrange
        var emptyElement = new TupleElement();

        // Act
        var firstChild = emptyElement.FirstChild;
        var lastChild = emptyElement.LastChild;

        // Assert
        Assert.IsNull(firstChild);
        Assert.IsNull(lastChild);
    }

    [TestMethod]
    public void Siblings_NoParent_ReturnsCorrectValues() {
        // Arrange
        var doc = CreateTestDocument();
        var root = doc.Root;
        var orphanElement = new StringElement("Orphan");

        // Act
        var rootSiblings = root.GetSiblings();
        var orphanSiblings = orphanElement.GetSiblings();

        // Assert
        Assert.AreEqual(0, rootSiblings.Count()); // Root has no siblings
        Assert.AreEqual(0, orphanSiblings.Count()); // Orphan has no siblings
    }

    [TestMethod]
    public void GetSiblings_ReturnsCorrectElements() {
        // Arrange
        var doc = CreateTestDocument();
        var header = doc.Root.FindElementById("header");

        // Act
        var siblings = header?.GetSiblings().ToList();

        // Assert
        Assert.IsNotNull(siblings);
        Assert.AreEqual(2, siblings.Count); // content and footer
        Assert.IsTrue(siblings.Any(s => s.Id == "content"));
        Assert.IsTrue(siblings.Any(s => s.Id == "footer"));
        Assert.IsFalse(siblings.Any(s => s.Id == "header")); // Should not include itself
    }

    [TestMethod]
    public void GetAncestors_ReturnsCorrectElements() {
        // Arrange
        var doc = CreateTestDocument();
        var title = doc.Root.FindElementById("title");

        // Act
        var ancestors = title?.GetAncestors().ToList();

        // Assert
        Assert.IsNotNull(ancestors);
        Assert.AreEqual(3, ancestors.Count); // KeyValuePair, ObjectElement (header), TupleElement (root)

        // Check the ancestor chain
        Assert.IsInstanceOfType(ancestors[0], typeof(KeyValuePairElement)); // Immediate parent
        Assert.AreEqual("header", ancestors[1].Id); // Header object
        Assert.IsInstanceOfType(ancestors[2], typeof(TupleElement)); // Root
    }

    [TestMethod]
    public void GetDescendants_ReturnsCorrectElements() {
        // Arrange
        var doc = CreateTestDocument();
        var header = doc.Root.FindElementById("header");

        // Act
        var descendants = header?.GetDescendants().ToList();

        // Assert
        Assert.IsNotNull(descendants);
        // Should include KeyValuePair elements and their children (title and subtitle)
        Assert.IsTrue(descendants.Count >= 4); // At least 2 KVP + 2 text elements

        var stringDescendants = descendants.OfType<StringElement>().ToList();
        Assert.AreEqual(2, stringDescendants.Count); // title and subtitle
        Assert.IsTrue(stringDescendants.Any(s => s.Id == "title"));
    }

    [TestMethod]
    public void GetDescendants_EmptyElement_ReturnsEmpty() {
        // Arrange
        var emptyElement = new TupleElement();

        // Act
        var descendants = emptyElement.GetDescendants().ToList();

        // Assert
        Assert.AreEqual(0, descendants.Count);
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void QueryAndNavigation_Integration_WorksTogether() {
        // Arrange
        var doc = CreateTestDocument();

        // Act - Find all items, then navigate to their siblings
        var itemElements = doc.Root.FindElementsByTag("item");
        var firstItem = itemElements.FirstOrDefault();
        var nextSibling = firstItem?.NextSibling;

        // Assert
        Assert.IsNotNull(firstItem);
        Assert.IsNotNull(nextSibling);
        Assert.AreEqual("item", firstItem.Tag);
        Assert.AreEqual("item", nextSibling.Tag);
        Assert.IsInstanceOfType(firstItem, typeof(StringElement));
        Assert.IsInstanceOfType(nextSibling, typeof(IntegerElement));
    }

    [TestMethod]
    public void ComplexNavigation_AcrossDocumentStructure() {
        // Arrange
        var doc = CreateTestDocument();

        // Act - Start from title, navigate to footer's copyright
        var title = doc.Root.FindElementById("title");
        var titleParent = title?.Parent; // KeyValuePair
        var headerParent = titleParent?.Parent; // Header object
        var rootParent = headerParent?.Parent; // Root
        var footer = rootParent?.FindElementById("footer");
        var copyrightElements = footer?.FindElementsByTag("text");
        var copyright = copyrightElements?.FirstOrDefault();

        // Assert
        Assert.IsNotNull(title);
        Assert.IsNotNull(copyright);
        Assert.AreEqual("title", title.Id);
        Assert.AreEqual("text", copyright.Tag);
        Assert.IsInstanceOfType(copyright, typeof(StringElement));
        Assert.AreEqual("© 2023", ((StringElement)copyright).Value);
    }

    #endregion
}
