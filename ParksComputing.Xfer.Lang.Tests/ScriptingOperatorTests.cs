using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ParksComputing.Xfer.Lang.Scripting;
using ParksComputing.Xfer.Lang.Scripting.Comparison;
using ParksComputing.Xfer.Lang.Scripting.Utility;
using ParksComputing.Xfer.Lang.Scripting.Logical;
using ParksComputing.Xfer.Lang.Elements;
using System.Collections.Generic;

namespace ParksComputing.Xfer.Lang.Tests;

/// <summary>
/// Tests for scripting operators that perform conditional logic and value evaluation.
/// Covers operator execution, argument validation, and edge case handling.
/// </summary>
[TestClass]
public class ScriptingOperatorTests {

    private ScriptingContext CreateTestContext() {
        var context = new ScriptingContext();
        context.Variables["TEST_STRING"] = "hello";
        context.Variables["TEST_NUMBER"] = 42;
        context.Variables["TEST_BOOLEAN"] = true;
        context.Variables["EMPTY_STRING"] = "";
        context.Variables["NULL_VALUE"] = null!;
        return context;
    }

    #region EqualsOperator Tests

    [TestMethod]
    public void EqualsOperator_Properties_AreCorrect() {
        // Arrange
        var op = new EqualsOperator();

        // Assert
        Assert.AreEqual("eq", op.OperatorName);
        Assert.AreEqual("Checks if two elements are equal", op.Description);
        Assert.AreEqual(2, op.MinArguments);
        Assert.AreEqual(2, op.MaxArguments);
    }

    [TestMethod]
    public void EqualsOperator_EqualStrings_ReturnsTrue() {
        // Arrange
        var op = new EqualsOperator();
        var context = CreateTestContext();
        var arg1 = new StringElement("hello");
        var arg2 = new StringElement("hello");

        // Act
        var result = op.Evaluate(context, arg1, arg2);

        // Assert
        Assert.IsTrue((bool)result!);
    }

    [TestMethod]
    public void EqualsOperator_DifferentStrings_ReturnsFalse() {
        // Arrange
        var op = new EqualsOperator();
        var context = CreateTestContext();
        var arg1 = new StringElement("hello");
        var arg2 = new StringElement("world");

        // Act
        var result = op.Evaluate(context, arg1, arg2);

        // Assert
        Assert.IsFalse((bool)result!);
    }

    [TestMethod]
    public void EqualsOperator_EqualIntegers_ReturnsTrue() {
        // Arrange
        var op = new EqualsOperator();
        var context = CreateTestContext();
        var arg1 = new IntegerElement(42);
        var arg2 = new IntegerElement(42);

        // Act
        var result = op.Evaluate(context, arg1, arg2);

        // Assert
        Assert.IsTrue((bool)result!);
    }

    [TestMethod]
    public void EqualsOperator_DifferentNumbers_ReturnsFalse() {
        // Arrange
        var op = new EqualsOperator();
        var context = CreateTestContext();
        var arg1 = new IntegerElement(42);
        var arg2 = new IntegerElement(24);

        // Act
        var result = op.Evaluate(context, arg1, arg2);

        // Assert
        Assert.IsFalse((bool)result!);
    }

    [TestMethod]
    public void EqualsOperator_EqualBooleans_ReturnsTrue() {
        // Arrange
        var op = new EqualsOperator();
        var context = CreateTestContext();
        var arg1 = new BooleanElement(true);
        var arg2 = new BooleanElement(true);

        // Act
        var result = op.Evaluate(context, arg1, arg2);

        // Assert
        Assert.IsTrue((bool)result!);
    }

    [TestMethod]
    public void EqualsOperator_BothNull_ReturnsTrue() {
        // Arrange
        var op = new EqualsOperator();
        var context = CreateTestContext();
        var arg1 = new EmptyElement();
        var arg2 = new EmptyElement();

        // Act
        var result = op.Evaluate(context, arg1, arg2);

        // Assert
        Assert.IsTrue((bool)result!);
    }

    [TestMethod]
    public void EqualsOperator_OneNullOneValue_ReturnsFalse() {
        // Arrange
        var op = new EqualsOperator();
        var context = CreateTestContext();
        var arg1 = new EmptyElement();
        var arg2 = new StringElement("hello");

        // Act
        var result = op.Evaluate(context, arg1, arg2);

        // Assert
        Assert.IsFalse((bool)result!);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void EqualsOperator_TooFewArguments_ThrowsException() {
        // Arrange
        var op = new EqualsOperator();
        var context = CreateTestContext();
        var arg1 = new StringElement("hello");

        // Act & Assert
        op.Evaluate(context, arg1);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void EqualsOperator_TooManyArguments_ThrowsException() {
        // Arrange
        var op = new EqualsOperator();
        var context = CreateTestContext();
        var arg1 = new StringElement("hello");
        var arg2 = new StringElement("world");
        var arg3 = new StringElement("extra");

        // Act & Assert
        op.Evaluate(context, arg1, arg2, arg3);
    }

    #endregion

    #region DefinedOperator Tests

    [TestMethod]
    public void DefinedOperator_Properties_AreCorrect() {
        // Arrange
        var op = new DefinedOperator();

        // Assert
        Assert.AreEqual("defined", op.OperatorName);
        Assert.AreEqual("Checks if an element has a meaningful value (is defined)", op.Description);
        Assert.AreEqual(1, op.MinArguments);
        Assert.AreEqual(1, op.MaxArguments);
    }

    [TestMethod]
    public void DefinedOperator_StringElementWithValue_ReturnsTrue() {
        // Arrange
        var op = new DefinedOperator();
        var context = CreateTestContext();
        var arg = new StringElement("hello");

        // Act
        var result = op.Evaluate(context, arg);

        // Assert
        Assert.IsTrue((bool)result!);
    }

    [TestMethod]
    public void DefinedOperator_EmptyStringElement_ReturnsTrue() {
        // Arrange
        var op = new DefinedOperator();
        var context = CreateTestContext();
        var arg = new StringElement("");

        // Act
        var result = op.Evaluate(context, arg);

        // Assert
        Assert.IsTrue((bool)result!); // Empty string is still defined
    }

    [TestMethod]
    public void DefinedOperator_IntegerElement_ReturnsTrue() {
        // Arrange
        var op = new DefinedOperator();
        var context = CreateTestContext();
        var arg = new IntegerElement(42);

        // Act
        var result = op.Evaluate(context, arg);

        // Assert
        Assert.IsTrue((bool)result!);
    }

    [TestMethod]
    public void DefinedOperator_IntegerElementZero_ReturnsTrue() {
        // Arrange
        var op = new DefinedOperator();
        var context = CreateTestContext();
        var arg = new IntegerElement(0);

        // Act
        var result = op.Evaluate(context, arg);

        // Assert
        Assert.IsTrue((bool)result!); // Zero is still defined
    }

    [TestMethod]
    public void DefinedOperator_BooleanElementFalse_ReturnsTrue() {
        // Arrange
        var op = new DefinedOperator();
        var context = CreateTestContext();
        var arg = new BooleanElement(false);

        // Act
        var result = op.Evaluate(context, arg);

        // Assert
        Assert.IsTrue((bool)result!); // False is still defined
    }

    [TestMethod]
    public void DefinedOperator_EmptyElement_ReturnsTrue() {
        // Arrange
        var op = new DefinedOperator();
        var context = CreateTestContext();
        var arg = new EmptyElement();

        // Act
        var result = op.Evaluate(context, arg);

        // Assert
        Assert.IsTrue((bool)result!); // Empty element is still defined
    }

    [TestMethod]
    public void DefinedOperator_NonEmptyCollectionElement_ReturnsTrue() {
        // Arrange
        var op = new DefinedOperator();
        var context = CreateTestContext();
        var arg = new TupleElement();
        arg.Add(new StringElement("item"));

        // Act
        var result = op.Evaluate(context, arg);

        // Assert
        Assert.IsTrue((bool)result!);
    }

    [TestMethod]
    public void DefinedOperator_EmptyCollectionElement_ReturnsTrue() {
        // Arrange
        var op = new DefinedOperator();
        var context = CreateTestContext();
        var arg = new TupleElement(); // Empty collection

        // Act
        var result = op.Evaluate(context, arg);

        // Assert
        Assert.IsTrue((bool)result!); // Empty collection is still defined
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void DefinedOperator_NoArguments_ThrowsException() {
        // Arrange
        var op = new DefinedOperator();
        var context = CreateTestContext();

        // Act & Assert
        op.Evaluate(context);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void DefinedOperator_TooManyArguments_ThrowsException() {
        // Arrange
        var op = new DefinedOperator();
        var context = CreateTestContext();
        var arg1 = new StringElement("hello");
        var arg2 = new StringElement("world");

        // Act & Assert
        op.Evaluate(context, arg1, arg2);
    }

    #endregion

    #region IfOperator Tests

    [TestMethod]
    public void IfOperator_Properties_AreCorrect() {
        // Arrange
        var op = new IfOperator();

        // Assert
        Assert.AreEqual("if", op.OperatorName);
        Assert.IsTrue(op.Description.Contains("condition"));
        Assert.AreEqual(2, op.MinArguments); // condition + true value
        Assert.AreEqual(3, op.MaxArguments); // condition + true value + false value (optional)
    }

    [TestMethod]
    public void IfOperator_TrueCondition_ReturnsThenBranch() {
        // Arrange
        var op = new IfOperator();
        var context = CreateTestContext();
        var condition = new BooleanElement(true);
        var thenBranch = new StringElement("then_result");
        var elseBranch = new StringElement("else_result");

        // Act
        var result = op.Evaluate(context, condition, thenBranch, elseBranch);

        // Assert
        Assert.AreEqual("then_result", (string)result!);
    }

    [TestMethod]
    public void IfOperator_FalseCondition_ReturnsElseBranch() {
        // Arrange
        var op = new IfOperator();
        var context = CreateTestContext();
        var condition = new BooleanElement(false);
        var thenBranch = new StringElement("then_result");
        var elseBranch = new StringElement("else_result");

        // Act
        var result = op.Evaluate(context, condition, thenBranch, elseBranch);

        // Assert
        Assert.AreEqual("else_result", (string)result!);
    }

    [TestMethod]
    public void IfOperator_NonBooleanTruthyCondition_ReturnsThenBranch() {
        // Arrange
        var op = new IfOperator();
        var context = CreateTestContext();
        var condition = new StringElement("non-empty");
        var thenBranch = new StringElement("then_result");
        var elseBranch = new StringElement("else_result");

        // Act
        var result = op.Evaluate(context, condition, thenBranch, elseBranch);

        // Assert
        Assert.AreEqual("then_result", (string)result!);
    }

    [TestMethod]
    public void IfOperator_NonBooleanFalsyCondition_ReturnsElseBranch() {
        // Arrange
        var op = new IfOperator();
        var context = CreateTestContext();
        var condition = new StringElement(""); // Empty string is falsy
        var thenBranch = new StringElement("then_result");
        var elseBranch = new StringElement("else_result");

        // Act
        var result = op.Evaluate(context, condition, thenBranch, elseBranch);

        // Assert
        Assert.AreEqual("else_result", (string)result!);
    }

    [TestMethod]
    public void IfOperator_TwoArguments_ValidOperation() {
        // Arrange
        var op = new IfOperator();
        var context = CreateTestContext();
        var condition = new BooleanElement(true);
        var thenBranch = new StringElement("then_result");

        // Act
        var result = op.Evaluate(context, condition, thenBranch);

        // Assert
        Assert.AreEqual("then_result", (string)result!);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void IfOperator_TooManyArguments_ThrowsException() {
        // Arrange
        var op = new IfOperator();
        var context = CreateTestContext();
        var condition = new BooleanElement(true);
        var thenBranch = new StringElement("then_result");
        var elseBranch = new StringElement("else_result");
        var extra = new StringElement("extra");

        // Act & Assert
        op.Evaluate(context, condition, thenBranch, elseBranch, extra);
    }

    #endregion

    #region Base Class Validation Tests

    [TestMethod]
    public void ScriptingOperator_ValidateArguments_ValidCount_DoesNotThrow() {
        // Arrange
        var op = new EqualsOperator();
        var arg1 = new StringElement("hello");
        var arg2 = new StringElement("world");

        // Act & Assert - Should not throw
        // Using reflection to test protected method
        var method = typeof(ScriptingOperator).GetMethod("ValidateArguments",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method!.Invoke(op, new object[] { new Element[] { arg1, arg2 } });
    }

    [TestMethod]
    public void ScriptingOperator_ValidateArguments_NullArguments_ThrowsException() {
        // Arrange
        var op = new EqualsOperator();

        // Act & Assert
        var method = typeof(ScriptingOperator).GetMethod("ValidateArguments",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var ex = Assert.ThrowsException<System.Reflection.TargetInvocationException>(() => {
            method!.Invoke(op, new object[] { null! });
        });

        Assert.IsInstanceOfType(ex.InnerException, typeof(ArgumentNullException));
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void OperatorIntegration_EqualsWithDefined_WorksTogether() {
        // Arrange
        var equalsOp = new EqualsOperator();
        var definedOp = new DefinedOperator();
        var context = CreateTestContext();

        var stringArg = new StringElement("hello");
        var emptyArg = new StringElement("");

        // Act
        var stringDefined = definedOp.Evaluate(context, stringArg);
        var emptyDefined = definedOp.Evaluate(context, emptyArg);
        var bothEqual = equalsOp.Evaluate(context, new BooleanElement((bool)stringDefined!), new BooleanElement((bool)emptyDefined!));

        // Assert
        Assert.IsTrue((bool)stringDefined!);
        Assert.IsTrue((bool)emptyDefined!); // Empty string is still defined
        Assert.IsTrue((bool)bothEqual!); // Both are true, so equal
    }

    [TestMethod]
    public void OperatorIntegration_IfWithEquals_WorksTogether() {
        // Arrange
        var ifOp = new IfOperator();
        var equalsOp = new EqualsOperator();
        var context = CreateTestContext();

        // First evaluate equals condition
        var condition = equalsOp.Evaluate(context, new StringElement("hello"), new StringElement("hello"));
        var thenResult = new StringElement("equal");
        var elseResult = new StringElement("not_equal");

        // Act - Use equals result in if statement
        var result = ifOp.Evaluate(context, new BooleanElement((bool)condition!), thenResult, elseResult);

        // Assert
        Assert.AreEqual("equal", (string)result!);
    }

    #endregion
}
