using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.ProcessingInstructions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ParksComputing.Xfer.Lang.Tests;

/// <summary>
/// Comprehensive tests for ProcessingInstruction base class functionality.
/// Tests virtual method behavior, element handling, and inheritance patterns.
/// </summary>
[TestClass]
public class ProcessingInstructionTests
{
    #region Test Helper Class

    /// <summary>
    /// Concrete implementation of ProcessingInstruction for testing abstract base class.
    /// </summary>
    private class TestProcessingInstruction : ProcessingInstruction
    {
        public const string TestProcessingInstructionName = "test";
        public static string ProcessingInstructionName => TestProcessingInstructionName;

        // Track method calls for testing
        public bool ElementHandlerCalled { get; private set; }
        public bool ProcessingInstructionHandlerCalled { get; private set; }
        public Element? LastHandledElement { get; private set; }

        public TestProcessingInstruction() : base(new StringElement("test"), TestProcessingInstructionName) { }

        public override void ElementHandler(Element element)
        {
            ElementHandlerCalled = true;
            LastHandledElement = element;
        }

        public override void ProcessingInstructionHandler()
        {
            ProcessingInstructionHandlerCalled = true;
        }

        // Reset for testing
        public void Reset()
        {
            ElementHandlerCalled = false;
            ProcessingInstructionHandlerCalled = false;
            LastHandledElement = null;
        }
    }

    #endregion

    #region Constructor Tests

    [TestMethod]
    public void Constructor_Default_SetsCorrectly()
    {
        // Arrange & Act
        var pi = new TestProcessingInstruction();

        // Assert
        Assert.AreEqual("test", TestProcessingInstruction.ProcessingInstructionName);
        Assert.AreEqual('!', pi.Delimiter.OpeningSpecifier);
        Assert.AreEqual('!', pi.Delimiter.ClosingSpecifier);
        Assert.AreEqual(ElementStyle.Explicit, pi.Delimiter.Style); // ProcessingInstructions use explicit style by default
        Assert.AreEqual(1, pi.Delimiter.SpecifierCount);
        Assert.AreEqual(0, pi.Children.Count);
        Assert.IsNull(pi.Parent);
    }

    #endregion

    #region Inheritance Tests

    [TestMethod]
    public void Inheritance_IsElement()
    {
        // Arrange & Act
        var pi = new TestProcessingInstruction();

        // Assert
        Assert.IsInstanceOfType(pi, typeof(Element));
        Assert.IsInstanceOfType(pi, typeof(ProcessingInstruction));
    }

    [TestMethod]
    public void Inheritance_HasElementProperties()
    {
        // Arrange
        var pi = new TestProcessingInstruction();

        // Act & Assert
        Assert.IsNotNull(pi.Delimiter);
        Assert.IsNotNull(pi.Children);
        Assert.IsNull(pi.Parent);
    }

    #endregion

    #region Method Implementation Tests

    [TestMethod]
    public void ProcessingInstructionName_ReturnsCorrectValue()
    {
        // Arrange & Act
        var name = TestProcessingInstruction.ProcessingInstructionName;

        // Assert
        Assert.AreEqual("test", name);
    }

    [TestMethod]
    public void ElementHandler_CalledWithElement_TracksCall()
    {
        // Arrange
        var pi = new TestProcessingInstruction();
        var element = new StringElement("test");

        // Act
        pi.ElementHandler(element);

        // Assert
        Assert.IsTrue(pi.ElementHandlerCalled);
        Assert.AreEqual(element, pi.LastHandledElement);
    }

    [TestMethod]
    public void ProcessingInstructionHandler_Called_TracksCall()
    {
        // Arrange
        var pi = new TestProcessingInstruction();

        // Act
        pi.ProcessingInstructionHandler();

        // Assert
        Assert.IsTrue(pi.ProcessingInstructionHandlerCalled);
    }

    #endregion

    #region Serialization Tests - Default Formatting

    [TestMethod]
    public void ToXfer_BasicProcessingInstruction_ReturnsCorrectFormat()
    {
        // Arrange
        var pi = new TestProcessingInstruction();

        // Act
        var result = pi.ToXfer();

        // Assert
        Assert.AreEqual("<!test\"test\"!>", result); // ProcessingInstructions use explicit format by default
    }

    [TestMethod]
    public void ToXfer_ExplicitStyle_ReturnsCorrectFormat()
    {
        // Arrange
        var pi = new TestProcessingInstruction();
        pi.Delimiter.Style = ElementStyle.Explicit;

        // Act
        var result = pi.ToXfer();

        // Assert
        Assert.AreEqual("<!test\"test\"!>", result);
    }

    [TestMethod]
    public void ToXfer_ImplicitStyle_ReturnsCorrectFormat()
    {
        // Arrange
        var pi = new TestProcessingInstruction();
        pi.Delimiter.Style = ElementStyle.Implicit;

        // Act
        var result = pi.ToXfer();

        // Assert
        Assert.AreEqual("test\"test\"", result);
    }

    #endregion

    #region Children Management Tests

    [TestMethod]
    public void AddChild_Element_AddsSuccessfully()
    {
        // Arrange
        var pi = new TestProcessingInstruction();
        var element = new StringElement("test");

        // Act
        pi.AddChild(element);

        // Assert
        Assert.AreEqual(1, pi.Children.Count);
        Assert.AreEqual(element, pi.Children.First());
        Assert.AreEqual(pi, element.Parent);
    }

    [TestMethod]
    public void AddChild_ProcessingInstruction_AddsSuccessfully()
    {
        // Arrange
        var pi1 = new TestProcessingInstruction();
        var pi2 = new TestProcessingInstruction();

        // Act
        pi1.AddChild(pi2);

        // Assert
        Assert.AreEqual(1, pi1.Children.Count);
        Assert.AreEqual(pi2, pi1.Children.First());
        Assert.AreEqual(pi1, pi2.Parent);
    }

    [TestMethod]
    public void RemoveChild_ExistingElement_RemovesSuccessfully()
    {
        // Arrange
        var pi = new TestProcessingInstruction();
        var element = new StringElement("test");
        pi.AddChild(element);

        // Act
        var result = pi.RemoveChild(element);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(0, pi.Children.Count);
        Assert.IsNull(element.Parent);
    }

    [TestMethod]
    public void RemoveChild_NonExistentElement_ReturnsFalse()
    {
        // Arrange
        var pi = new TestProcessingInstruction();
        var element = new StringElement("test");

        // Act
        var result = pi.RemoveChild(element);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void RemoveAllChildren_WithMultipleChildren_RemovesAll()
    {
        // Arrange
        var pi = new TestProcessingInstruction();
        var element1 = new StringElement("test1");
        var element2 = new StringElement("test2");
        var element3 = new StringElement("test3");
        pi.AddChild(element1);
        pi.AddChild(element2);
        pi.AddChild(element3);

        // Act
        var removedCount = pi.RemoveAllChildren();

        // Assert
        Assert.AreEqual(3, removedCount);
        Assert.AreEqual(0, pi.Children.Count);
        Assert.IsNull(element1.Parent);
        Assert.IsNull(element2.Parent);
        Assert.IsNull(element3.Parent);
    }

    #endregion

    #region Serialization Tests - With Children

    [TestMethod]
    public void ToXfer_WithStringChild_ReturnsCorrectFormat()
    {
        // Arrange
        var pi = new TestProcessingInstruction();
        pi.AddChild(new StringElement("hello"));

        // Act
        var result = pi.ToXfer();

        // Assert
        Assert.AreEqual("<!test\"test\"!>", result); // ProcessingInstruction only outputs its core KVP
    }

    [TestMethod]
    public void ToXfer_WithMultipleChildren_ReturnsCorrectFormat()
    {
        // Arrange
        var pi = new TestProcessingInstruction();
        pi.AddChild(new StringElement("hello"));
        pi.AddChild(new IntegerElement(42));

        // Act
        var result = pi.ToXfer();

        // Assert
        Assert.AreEqual("<!test\"test\"!>", result); // ProcessingInstruction only outputs its core KVP, children are ignored
    }

    [TestMethod]
    public void ToXfer_WithIndentation_ReturnsFormattedOutput()
    {
        // Arrange
        var pi = new TestProcessingInstruction();
        pi.AddChild(new StringElement("hello"));
        pi.AddChild(new IntegerElement(42));

        // Act
        var result = pi.ToXfer(Formatting.Indented);

        // Assert
        var expected = $"<!{Environment.NewLine}  test\"test\"{Environment.NewLine}!>";
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ToXfer_WithCustomIndentation_ReturnsFormattedOutput()
    {
        // Arrange
        var pi = new TestProcessingInstruction();
        pi.AddChild(new StringElement("hello"));

        // Act
        var result = pi.ToXfer(Formatting.Indented, indentChar: '\t', indentation: 1);

        // Assert
        var expected = $"<!{Environment.NewLine}\ttest\"test\"{Environment.NewLine}!>";
        Assert.AreEqual(expected, result);
    }

    #endregion

    #region Parent-Child Relationship Tests

    [TestMethod]
    public void Parent_SetAndGet_WorksCorrectly()
    {
        // Arrange
        var parent = new ObjectElement();
        var pi = new TestProcessingInstruction();

        // Act
        parent.AddOrUpdate(pi);

        // Assert
        Assert.AreEqual(parent, pi.Parent);
        Assert.IsTrue(parent.Children.Contains(pi));
    }

    [TestMethod]
    public void Parent_WhenAddedToMultipleParents_UpdatesCorrectly()
    {
        // Arrange
        var parent1 = new ObjectElement();
        var parent2 = new ObjectElement();
        var pi = new TestProcessingInstruction();

        // Act
        parent1.AddOrUpdate(pi);
        parent2.AddOrUpdate(pi);

        // Assert - When an element is added to a new parent, it should be removed from the old parent
        Assert.AreEqual(parent2, pi.Parent);
        Assert.IsFalse(parent1.Children.Contains(pi), "ProcessingInstruction should be removed from previous parent when added to new parent");
        Assert.IsTrue(parent2.Children.Contains(pi), "ProcessingInstruction should be in new parent's children collection");
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_ReturnsXferRepresentation()
    {
        // Arrange
        var pi = new TestProcessingInstruction();

        // Act
        var result = pi.ToString();

        // Assert
        Assert.AreEqual("test", result);
    }

    [TestMethod]
    public void ToString_WithChildren_ReturnsCorrectFormat()
    {
        // Arrange
        var pi = new TestProcessingInstruction();
        pi.AddChild(new StringElement("hello"));

        // Act
        var result = pi.ToString();

        // Assert
        // ToString() returns the base value, not the full representation with children
        Assert.AreEqual("test", result);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void EdgeCases_NullElementHandler_HandlesCorrectly()
    {
        // Arrange
        var pi = new TestProcessingInstruction();

        // Act & Assert - Should not throw
        pi.ElementHandler(null!);
        Assert.IsTrue(pi.ElementHandlerCalled);
        Assert.IsNull(pi.LastHandledElement);
    }

    [TestMethod]
    public void EdgeCases_ProcessingInstructionHandler_HandlesCorrectly()
    {
        // Arrange
        var pi = new TestProcessingInstruction();

        // Act & Assert - Should not throw
        pi.ProcessingInstructionHandler();
        Assert.IsTrue(pi.ProcessingInstructionHandlerCalled);
    }

    #endregion

    #region Handler Behavior Tests

    [TestMethod]
    public void HandlerBehavior_ElementHandler_CanAccessElementProperties()
    {
        // Arrange
        var pi = new TestProcessingInstruction();
        var stringElement = new StringElement("test value");

        // Act
        pi.ElementHandler(stringElement);

        // Assert
        Assert.AreEqual(stringElement, pi.LastHandledElement);
        Assert.IsInstanceOfType(pi.LastHandledElement, typeof(StringElement));
        Assert.AreEqual("test value", ((StringElement)pi.LastHandledElement).Value);
    }

    [TestMethod]
    public void HandlerBehavior_ProcessingInstructionHandler_CallsCorrectly()
    {
        // Arrange
        var pi = new TestProcessingInstruction();

        // Act
        pi.ProcessingInstructionHandler();

        // Assert
        Assert.IsTrue(pi.ProcessingInstructionHandlerCalled);
    }

    #endregion

    #region Virtual Method Override Tests

    [TestMethod]
    public void VirtualMethods_CanBeOverridden_BehaveDifferently()
    {
        // This test validates that our test implementation correctly overrides the base virtual methods

        // Arrange
        var pi = new TestProcessingInstruction();
        var element = new StringElement("test");

        // Act
        pi.ElementHandler(element);
        pi.ProcessingInstructionHandler();

        // Assert
        Assert.IsTrue(pi.ElementHandlerCalled);
        Assert.IsTrue(pi.ProcessingInstructionHandlerCalled);
    }

    #endregion

    #region Realistic Use Cases

    [TestMethod]
    public void RealisticUseCases_CustomProcessingInstruction_WorksCorrectly()
    {
        // Simulate a custom processing instruction that processes configuration

        // Arrange
        var configPi = new TestProcessingInstruction();
        configPi.AddChild(new StringElement("config"));
        configPi.AddChild(new StringElement("server"));
        configPi.AddChild(new StringElement("localhost"));

        // Act
        var xfer = configPi.ToXfer();

        // Assert
        Assert.IsTrue(xfer.Contains("test\"test\""));
        Assert.IsTrue(xfer.Contains("\"config\""));
        Assert.IsTrue(xfer.Contains("\"server\""));
        Assert.IsTrue(xfer.Contains("\"localhost\""));
    }

    [TestMethod]
    public void RealisticUseCases_ProcessingInstructionWithComplexChildren_WorksCorrectly()
    {
        // Arrange
        var pi = new TestProcessingInstruction();

        var objectChild = new ObjectElement();
        objectChild["type"] = new StringElement("validation");
        objectChild["required"] = new BooleanElement(true);

        pi.AddChild(objectChild);

        // Act
        var xfer = pi.ToXfer();

        // Assert - ProcessingInstruction only outputs its core KVP, not added children
        Assert.AreEqual("<!test\"test\"!>", xfer);
    }

    #endregion
}
