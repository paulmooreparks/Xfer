using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Tests;

/// <summary>
/// Comprehensive tests for DOM-like element removal capabilities.
/// Tests removal methods across all element types and hierarchical scenarios.
/// </summary>
[TestClass]
public class ElementRemovalTests {

    [TestMethod]
    public void Element_Remove_RemovesFromParent() {
        // Arrange
        var parent = new TupleElement();
        var child = new StringElement("test");
        parent.Add(child);

        // Act
        bool result = child.Remove();

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(child.Parent);
        Assert.AreEqual(0, parent.Count);
        Assert.IsFalse(parent.Children.Contains(child));
    }

    [TestMethod]
    public void Element_Remove_ReturnsFalseWhenNoParent() {
        // Arrange
        var element = new StringElement("test");

        // Act
        bool result = element.Remove();

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(element.Parent);
    }

    [TestMethod]
    public void Element_RemoveChild_RemovesAndClearsParent() {
        // Arrange
        var parent = new TupleElement();
        var child = new StringElement("test");
        parent.Add(child);

        // Act
        bool result = parent.RemoveChild(child);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(child.Parent);
        Assert.AreEqual(0, parent.Count);
        Assert.IsFalse(parent.Children.Contains(child));
    }

    [TestMethod]
    public void Element_RemoveChildAt_RemovesCorrectChild() {
        // Arrange
        var parent = new TupleElement();
        var child1 = new StringElement("first");
        var child2 = new StringElement("second");
        var child3 = new StringElement("third");
        parent.Add(child1);
        parent.Add(child2);
        parent.Add(child3);

        // Act
        bool result = parent.RemoveChildAt(1); // Remove "second"

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(child2.Parent);
        Assert.AreEqual(2, parent.Count);
        Assert.IsTrue(parent.Children.Contains(child1));
        Assert.IsFalse(parent.Children.Contains(child2));
        Assert.IsTrue(parent.Children.Contains(child3));
    }

    [TestMethod]
    public void Element_RemoveChildAt_ReturnsFalseForInvalidIndex() {
        // Arrange
        var parent = new TupleElement();
        var child = new StringElement("test");
        parent.Add(child);

        // Act & Assert
        Assert.IsFalse(parent.RemoveChildAt(-1));
        Assert.IsFalse(parent.RemoveChildAt(1));
        Assert.IsFalse(parent.RemoveChildAt(10));

        // Child should still be there
        Assert.AreEqual(1, parent.Count);
        Assert.AreEqual(parent, child.Parent);
    }

    [TestMethod]
    public void Element_RemoveAllChildren_ClearsAllChildren() {
        // Arrange
        var parent = new TupleElement();
        var child1 = new StringElement("first");
        var child2 = new StringElement("second");
        var child3 = new StringElement("third");
        parent.Add(child1);
        parent.Add(child2);
        parent.Add(child3);

        // Act
        int removedCount = parent.RemoveAllChildren();

        // Assert
        Assert.AreEqual(3, removedCount);
        Assert.AreEqual(0, parent.Count);
        Assert.AreEqual(0, parent.Children.Count);
        Assert.IsNull(child1.Parent);
        Assert.IsNull(child2.Parent);
        Assert.IsNull(child3.Parent);
    }

    [TestMethod]
    public void Element_ReplaceChild_ReplacesSuccessfully() {
        // Arrange
        var parent = new TupleElement();
        var oldChild = new StringElement("old");
        var newChild = new StringElement("new");
        parent.Add(oldChild);

        // Act
        bool result = parent.ReplaceChild(oldChild, newChild);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(oldChild.Parent);
        Assert.AreEqual(parent, newChild.Parent);
        Assert.AreEqual(1, parent.Count);
        Assert.IsFalse(parent.Children.Contains(oldChild));
        Assert.IsTrue(parent.Children.Contains(newChild));
    }

    [TestMethod]
    public void Element_ReplaceChild_ReturnsFalseForNonExistentChild() {
        // Arrange
        var parent = new TupleElement();
        var existingChild = new StringElement("existing");
        var nonExistentChild = new StringElement("nonexistent");
        var newChild = new StringElement("new");
        parent.Add(existingChild);

        // Act
        bool result = parent.ReplaceChild(nonExistentChild, newChild);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(1, parent.Count);
        Assert.IsTrue(parent.Children.Contains(existingChild));
        Assert.IsFalse(parent.Children.Contains(newChild));
        Assert.IsNull(newChild.Parent);
    }

    [TestMethod]
    public void ListElement_RemoveChild_HandlesItemsAndChildren() {
        // Arrange
        var list = new ArrayElement();
        var element1 = new StringElement("first");
        var element2 = new StringElement("second");
        list.Add(element1);
        list.Add(element2);

        // Act
        bool result = list.RemoveChild(element1);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(element1.Parent);
        Assert.AreEqual(1, list.Count);
        Assert.IsFalse(list.Children.Contains(element1));
        Assert.IsTrue(list.Children.Contains(element2));
    }

    [TestMethod]
    public void ArrayElement_RemoveAllChildren_ResetsElementType() {
        // Arrange
        var array = new ArrayElement();
        array.Add(new StringElement("first"));
        array.Add(new StringElement("second"));

        // Act
        int removedCount = array.RemoveAllChildren();

        // Assert
        Assert.AreEqual(2, removedCount);
        Assert.AreEqual(0, array.Count);
        Assert.IsNull(array.ElementType); // Should reset when empty
    }

    [TestMethod]
    public void ObjectElement_RemoveChild_RemovesKVPFromDictionary() {
        // Arrange
        var obj = new ObjectElement();
        var kvp = new KeyValuePairElement(new KeywordElement("testKey"), new StringElement("testValue"));
        obj.AddOrUpdate(kvp);

        // Act
        bool result = obj.RemoveChild(kvp);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(kvp.Parent);
        Assert.AreEqual(0, obj.Count);
        Assert.IsFalse(obj.ContainsKey("testKey"));
        Assert.IsFalse(obj.Children.Contains(kvp));
    }

    [TestMethod]
    public void ObjectElement_Remove_RemovesByKey() {
        // Arrange
        var obj = new ObjectElement();
        var kvp = new KeyValuePairElement(new KeywordElement("testKey"), new StringElement("testValue"));
        obj.AddOrUpdate(kvp);

        // Act
        bool result = obj.Remove("testKey");

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(kvp.Parent);
        Assert.AreEqual(0, obj.Count);
        Assert.IsFalse(obj.ContainsKey("testKey"));
    }

    [TestMethod]
    public void ObjectElement_ReplaceChild_ReplacesKVPWithSameKey() {
        // Arrange
        var obj = new ObjectElement();
        var oldKvp = new KeyValuePairElement(new KeywordElement("testKey"), new StringElement("oldValue"));
        var newKvp = new KeyValuePairElement(new KeywordElement("testKey"), new StringElement("newValue"));
        obj.AddOrUpdate(oldKvp);

        // Act
        bool result = obj.ReplaceChild(oldKvp, newKvp);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(1, obj.Count);
        Assert.IsTrue(obj.ContainsKey("testKey"));
        Assert.AreEqual("newValue", ((StringElement)obj.GetElement("testKey")).Value);
        Assert.AreEqual(obj, newKvp.Parent);
    }

    [TestMethod]
    public void DeepHierarchy_Remove_MaintainsStructure() {
        // Arrange - Create a deep hierarchy
        var root = new ObjectElement();
        var level1 = new ArrayElement();
        var level2 = new TupleElement();
        var leaf = new StringElement("leaf");

        root.AddOrUpdate(new KeyValuePairElement(new KeywordElement("level1"), level1));
        level1.Add(level2);
        level2.Add(leaf);

        // Act - Remove leaf from deep hierarchy
        bool result = leaf.Remove();

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(leaf.Parent);
        Assert.AreEqual(0, level2.Count);
        Assert.AreEqual(1, level1.Count); // level2 still in level1
        Assert.AreEqual(1, root.Count);   // level1 still in root

        // Verify structure integrity
        Assert.AreEqual(level1.Parent, root.Children[0]); // level1's parent is the KVP in root
        Assert.AreEqual(level1, level2.Parent);
    }

    [TestMethod]
    public void MixedElementTypes_RemovalOperations_WorkCorrectly() {
        // Arrange - Create a complex mixed structure
        var root = new TupleElement();
        var obj = new ObjectElement();
        var array = new ArrayElement();
        var nestedTuple = new TupleElement();

        root.Add(obj);
        root.Add(array);
        obj.AddOrUpdate(new KeyValuePairElement(new KeywordElement("nested"), nestedTuple));
        array.Add(new StringElement("arrayItem"));
        nestedTuple.Add(new BooleanElement(true));

        // Act & Assert - Test various removal operations

        // Remove from array
        Assert.IsTrue(array.RemoveChildAt(0));
        Assert.AreEqual(0, array.Count);

        // Remove from object
        Assert.IsTrue(obj.Remove("nested"));
        Assert.AreEqual(0, obj.Count);
        Assert.IsNull(nestedTuple.Parent);

        // Remove remaining children from root
        Assert.AreEqual(2, root.RemoveAllChildren());
        Assert.AreEqual(0, root.Count);
        Assert.IsNull(obj.Parent);
        Assert.IsNull(array.Parent);
    }

    [TestMethod]
    public void ParentChildRelationships_MaintainedThroughOperations() {
        // Arrange
        var parent = new TupleElement();
        var child1 = new StringElement("child1");
        var child2 = new StringElement("child2");
        var child3 = new StringElement("child3");

        parent.Add(child1);
        parent.Add(child2);
        parent.Add(child3);

        // Act & Assert - Test that parent-child relationships are properly maintained

        // Remove middle child
        Assert.IsTrue(child2.Remove());
        Assert.IsNull(child2.Parent);
        Assert.AreEqual(parent, child1.Parent);
        Assert.AreEqual(parent, child3.Parent);
        Assert.AreEqual(2, parent.Count);

        // Replace a child
        var newChild = new StringElement("replacement");
        Assert.IsTrue(parent.ReplaceChild(child1, newChild));
        Assert.IsNull(child1.Parent);
        Assert.AreEqual(parent, newChild.Parent);
        Assert.AreEqual(parent, child3.Parent);

        // Verify final state
        Assert.AreEqual(2, parent.Count);
        Assert.IsTrue(parent.Children.Contains(newChild));
        Assert.IsTrue(parent.Children.Contains(child3));
        Assert.IsFalse(parent.Children.Contains(child1));
        Assert.IsFalse(parent.Children.Contains(child2));
    }
}
