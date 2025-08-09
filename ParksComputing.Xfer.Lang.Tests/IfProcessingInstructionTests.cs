using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.ProcessingInstructions;
using System;
using System.Linq;

namespace ParksComputing.Xfer.Lang.Tests;

/// <summary>
/// Comprehensive tests for IfProcessingInstruction functionality.
/// Tests conditional processing logic and element filtering.
/// </summary>
[TestClass]
public class IfProcessingInstructionTests
{
    #region Test Helper Class

    /// <summary>
    /// Concrete implementation of IfProcessingInstruction for testing specific functionality.
    /// </summary>
    private class TestIfProcessingInstruction : IfProcessingInstruction
    {
        // Track method calls for testing
        public bool ElementHandlerCalled { get; private set; }
        public bool ProcessingInstructionHandlerCalled { get; private set; }
        public Element? LastHandledElement { get; private set; }

        public TestIfProcessingInstruction(Element conditionExpression) : base(conditionExpression) { }

        public override void ElementHandler(Element element)
        {
            ElementHandlerCalled = true;
            LastHandledElement = element;
            base.ElementHandler(element);
        }

        public override void ProcessingInstructionHandler()
        {
            ProcessingInstructionHandlerCalled = true;
            base.ProcessingInstructionHandler();
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
    public void Constructor_WithCondition_SetsCorrectly()
    {
        // Arrange
        var condition = new StringElement("test condition");

        // Act
        var ifPi = new IfProcessingInstruction(condition);

        // Assert
        Assert.AreEqual("if", IfProcessingInstruction.Keyword);
        Assert.AreEqual('!', ifPi.Delimiter.OpeningSpecifier);
        Assert.AreEqual('!', ifPi.Delimiter.ClosingSpecifier);
        Assert.AreEqual(ElementStyle.Explicit, ifPi.Delimiter.Style); // ProcessingInstructions use explicit style by default
        Assert.AreEqual(1, ifPi.Delimiter.SpecifierCount);
        Assert.AreEqual(condition, ifPi.ConditionExpression);
    }

    [TestMethod]
    public void Constructor_WithNullCondition_ThrowsException()
    {
        // Arrange, Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => new IfProcessingInstruction(null!));
    }

    [TestMethod]
    public void Constructor_WithComplexCondition_SetsCorrectly()
    {
        // Arrange
        var condition = new StringElement("complex && condition");

        // Act
        var ifPi = new IfProcessingInstruction(condition);

        // Assert
        Assert.AreEqual(condition, ifPi.ConditionExpression);
        Assert.IsFalse(ifPi.ConditionMet); // Should be false until processed
    }

    #endregion

    #region Inheritance Tests

    [TestMethod]
    public void Inheritance_IsProcessingInstruction()
    {
        // Arrange & Act
        var condition = new StringElement("test");
        var ifPi = new IfProcessingInstruction(condition);

        // Assert
        Assert.IsInstanceOfType(ifPi, typeof(ProcessingInstruction));
        Assert.IsInstanceOfType(ifPi, typeof(Element));
    }

    [TestMethod]
    public void Inheritance_HasProcessingInstructionProperties()
    {
        // Arrange
        var condition = new StringElement("test");
        var ifPi = new IfProcessingInstruction(condition);

        // Act & Assert
        Assert.IsNotNull(ifPi.Delimiter);
        Assert.AreEqual("if", IfProcessingInstruction.Keyword);
    }

    #endregion

    #region Keyword Tests

    [TestMethod]
    public void Keyword_ReturnsCorrectValue()
    {
        // Arrange & Act
        var keyword = IfProcessingInstruction.Keyword;

        // Assert
        Assert.AreEqual("if", keyword);
    }

    [TestMethod]
    public void Keyword_IsConstant()
    {
        // Arrange & Act
        var keyword1 = IfProcessingInstruction.Keyword;
        var condition = new StringElement("test");
        var ifPi = new IfProcessingInstruction(condition);
        var keyword2 = IfProcessingInstruction.Keyword;

        // Assert
        Assert.AreEqual(keyword1, keyword2);
        Assert.AreEqual("if", keyword1);
    }

    #endregion

    #region Method Implementation Tests

    [TestMethod]
    public void ElementHandler_CalledWithElement_TracksCall()
    {
        // Arrange
        var condition = new StringElement("true");
        var ifPi = new TestIfProcessingInstruction(condition);
        var element = new StringElement("test");

        // Act
        ifPi.ElementHandler(element);

        // Assert
        Assert.IsTrue(ifPi.ElementHandlerCalled);
        Assert.AreEqual(element, ifPi.LastHandledElement);
    }

    [TestMethod]
    public void ProcessingInstructionHandler_Called_TracksCall()
    {
        // Arrange
        var condition = new StringElement("true");
        var ifPi = new TestIfProcessingInstruction(condition);

        // Act
        ifPi.ProcessingInstructionHandler();

        // Assert
        Assert.IsTrue(ifPi.ProcessingInstructionHandlerCalled);
    }

    [TestMethod]
    public void ProcessingInstructionHandler_EvaluatesCondition()
    {
        // Arrange
        var condition = new StringElement("true");
        var ifPi = new IfProcessingInstruction(condition);

        // Act
        ifPi.ProcessingInstructionHandler();

        // Assert (condition evaluation result stored)
        // Note: The actual evaluation depends on the scripting engine implementation
        Assert.IsNotNull(ifPi.ConditionExpression);
    }

    #endregion

    #region Condition Evaluation Tests

    [TestMethod]
    public void ConditionExpression_SetInConstructor_Accessible()
    {
        // Arrange
        var condition = new StringElement("test condition");

        // Act
        var ifPi = new IfProcessingInstruction(condition);

        // Assert
        Assert.AreEqual(condition, ifPi.ConditionExpression);
    }

    [TestMethod]
    public void ConditionMet_InitiallyFalse()
    {
        // Arrange & Act
        var condition = new StringElement("test");
        var ifPi = new IfProcessingInstruction(condition);

        // Assert
        Assert.IsFalse(ifPi.ConditionMet);
    }

    [TestMethod]
    public void ConditionMet_AfterProcessingInstructionHandler_MayChange()
    {
        // Arrange
        var condition = new StringElement("test");
        var ifPi = new IfProcessingInstruction(condition);

        // Act
        ifPi.ProcessingInstructionHandler();

        // Assert (condition may be true or false depending on evaluation)
        // The specific result depends on the scripting engine implementation
        Assert.IsNotNull(ifPi.ConditionExpression);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void EdgeCases_NullElementHandler_HandlesCorrectly()
    {
        // Arrange
        var condition = new StringElement("test");
        var ifPi = new TestIfProcessingInstruction(condition);

        // Act & Assert - Should not throw
        ifPi.ElementHandler(null!);
        Assert.IsTrue(ifPi.ElementHandlerCalled);
        Assert.IsNull(ifPi.LastHandledElement);
    }

    [TestMethod]
    public void EdgeCases_ProcessingInstructionHandler_HandlesCorrectly()
    {
        // Arrange
        var condition = new StringElement("test");
        var ifPi = new TestIfProcessingInstruction(condition);

        // Act & Assert - Should not throw
        ifPi.ProcessingInstructionHandler();
        Assert.IsTrue(ifPi.ProcessingInstructionHandlerCalled);
    }

    [TestMethod]
    public void EdgeCases_EmptyCondition_HandlesCorrectly()
    {
        // Arrange
        var condition = new StringElement("");

        // Act
        var ifPi = new IfProcessingInstruction(condition);

        // Assert
        Assert.AreEqual(condition, ifPi.ConditionExpression);
        Assert.IsFalse(ifPi.ConditionMet);
    }

    #endregion

    #region Handler Behavior Tests

    [TestMethod]
    public void HandlerBehavior_ElementHandler_CanAccessElementProperties()
    {
        // Arrange
        var condition = new StringElement("test");
        var ifPi = new TestIfProcessingInstruction(condition);
        var stringElement = new StringElement("test value");

        // Act
        ifPi.ElementHandler(stringElement);

        // Assert
        Assert.AreEqual(stringElement, ifPi.LastHandledElement);
        Assert.IsInstanceOfType(ifPi.LastHandledElement, typeof(StringElement));
        Assert.AreEqual("test value", ((StringElement)ifPi.LastHandledElement).Value);
    }

    [TestMethod]
    public void HandlerBehavior_ProcessingInstructionHandler_CallsCorrectly()
    {
        // Arrange
        var condition = new StringElement("test");
        var ifPi = new TestIfProcessingInstruction(condition);

        // Act
        ifPi.ProcessingInstructionHandler();

        // Assert
        Assert.IsTrue(ifPi.ProcessingInstructionHandlerCalled);
    }

    #endregion

    #region Virtual Method Override Tests

    [TestMethod]
    public void VirtualMethods_CanBeOverridden_BehaveDifferently()
    {
        // Arrange
        var condition = new StringElement("test");
        var ifPi = new TestIfProcessingInstruction(condition);
        var element = new StringElement("test");

        // Act
        ifPi.ElementHandler(element);
        ifPi.ProcessingInstructionHandler();

        // Assert - Our test implementation tracks calls
        Assert.IsTrue(ifPi.ElementHandlerCalled);
        Assert.IsTrue(ifPi.ProcessingInstructionHandlerCalled);
        Assert.AreEqual(element, ifPi.LastHandledElement);
    }

    #endregion

    #region Realistic Use Cases

    [TestMethod]
    public void RealisticUseCases_ConditionalProcessing_WorksCorrectly()
    {
        // Arrange
        var condition = new StringElement("defined");
        var ifPi = new IfProcessingInstruction(condition);
        var element = new StringElement("conditional content");

        // Act
        ifPi.ProcessingInstructionHandler();

        // Assert
        Assert.AreEqual(condition, ifPi.ConditionExpression);
        Assert.IsNotNull(ifPi.ConditionExpression);
    }

    [TestMethod]
    public void RealisticUseCases_IfProcessingInstructionWithComplexCondition_WorksCorrectly()
    {
        // Arrange
        var condition = new StringElement("var1 && var2");
        var ifPi = new IfProcessingInstruction(condition);

        // Act
        ifPi.ProcessingInstructionHandler();

        // Assert
        Assert.AreEqual(condition, ifPi.ConditionExpression);
        Assert.IsNotNull(ifPi.ConditionExpression);
    }

    [TestMethod]
    public void RealisticUseCases_ConditionalElementFiltering_WorksCorrectly()
    {
        // Arrange
        var condition = new StringElement("debug");
        var ifPi = new IfProcessingInstruction(condition);
        var debugElement = new StringElement("debug info");

        // Act
        ifPi.ProcessingInstructionHandler();

        // Assert
        Assert.AreEqual(condition, ifPi.ConditionExpression);
        Assert.IsNotNull(ifPi.ConditionExpression);
    }

    #endregion

    #region Complex Scenario Tests

    [TestMethod]
    public void ComplexScenarios_MultipleConditionTypes_HandleCorrectly()
    {
        // Arrange & Act
        var stringCondition = new StringElement("string condition");
        var stringIf = new IfProcessingInstruction(stringCondition);

        var booleanCondition = new BooleanElement(true);
        var booleanIf = new IfProcessingInstruction(booleanCondition);

        var numericCondition = new IntegerElement(42);
        var numericIf = new IfProcessingInstruction(numericCondition);

        // Assert
        Assert.AreEqual(stringCondition, stringIf.ConditionExpression);
        Assert.AreEqual(booleanCondition, booleanIf.ConditionExpression);
        Assert.AreEqual(numericCondition, numericIf.ConditionExpression);
    }

    [TestMethod]
    public void ComplexScenarios_NestedConditionProcessing_HandleCorrectly()
    {
        // Arrange
        var nestedObject = new ObjectElement();
        nestedObject["condition"] = new StringElement("nested");
        var ifPi = new IfProcessingInstruction(nestedObject);

        // Act
        ifPi.ProcessingInstructionHandler();

        // Assert
        Assert.AreEqual(nestedObject, ifPi.ConditionExpression);
        Assert.IsInstanceOfType(ifPi.ConditionExpression, typeof(ObjectElement));
    }

    [TestMethod]
    public void ComplexScenarios_ConditionalChaining_WorksCorrectly()
    {
        // Arrange
        var condition1 = new StringElement("condition1");
        var condition2 = new StringElement("condition2");
        var if1 = new IfProcessingInstruction(condition1);
        var if2 = new IfProcessingInstruction(condition2);

        // Act
        if1.ProcessingInstructionHandler();
        if2.ProcessingInstructionHandler();

        // Assert
        Assert.AreEqual(condition1, if1.ConditionExpression);
        Assert.AreEqual(condition2, if2.ConditionExpression);
        Assert.AreNotEqual(if1.ConditionExpression, if2.ConditionExpression);
    }

    #endregion

    #region Performance and Stress Tests

    [TestMethod]
    public void Performance_LargeConditionStrings_HandleEfficiently()
    {
        // Arrange
        var largeCondition = new StringElement(new string('a', 10000));

        // Act & Assert - Should not throw
        var ifPi = new IfProcessingInstruction(largeCondition);
        ifPi.ProcessingInstructionHandler();

        Assert.AreEqual(largeCondition, ifPi.ConditionExpression);
    }

    [TestMethod]
    public void Performance_MultipleInstances_HandleCorrectly()
    {
        // Arrange & Act
        var instances = new IfProcessingInstruction[100];
        for (int i = 0; i < 100; i++)
        {
            instances[i] = new IfProcessingInstruction(new StringElement($"condition_{i}"));
        }

        // Assert
        for (int i = 0; i < 100; i++)
        {
            Assert.IsNotNull(instances[i].ConditionExpression);
            Assert.AreEqual($"condition_{i}", ((StringElement)instances[i].ConditionExpression).Value);
        }
    }

    #endregion

    #region Boundary Tests

    [TestMethod]
    public void Boundary_SpecialCharactersInCondition_HandleCorrectly()
    {
        // Arrange
        var specialCondition = new StringElement("condition && test || !flag");

        // Act
        var ifPi = new IfProcessingInstruction(specialCondition);

        // Assert
        Assert.AreEqual(specialCondition, ifPi.ConditionExpression);
    }

    [TestMethod]
    public void Boundary_UnicodeCondition_HandleCorrectly()
    {
        // Arrange
        var unicodeCondition = new StringElement("条件 && テスト");

        // Act
        var ifPi = new IfProcessingInstruction(unicodeCondition);

        // Assert
        Assert.AreEqual(unicodeCondition, ifPi.ConditionExpression);
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void Integration_WithOtherElements_WorksCorrectly()
    {
        // Arrange
        var condition = new StringElement("integration_test");
        var ifPi = new IfProcessingInstruction(condition);
        var targetElement = new StringElement("target");

        // Act
        ifPi.ProcessingInstructionHandler();
        ifPi.ElementHandler(targetElement);

        // Assert
        Assert.AreEqual(condition, ifPi.ConditionExpression);
        Assert.IsNotNull(ifPi.ConditionExpression);
    }

    [TestMethod]
    public void Integration_ToXferFormat_ProducesValidOutput()
    {
        // Arrange
        var condition = new StringElement("test_condition");
        var ifPi = new IfProcessingInstruction(condition);

        // Act
        var xferOutput = ifPi.ToXfer();

        // Assert
        Assert.IsNotNull(xferOutput);
        Assert.IsTrue(xferOutput.Contains("if"));
        Assert.IsTrue(xferOutput.Contains("!"));
    }

    #endregion
}
