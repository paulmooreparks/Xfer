using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using ParksComputing.Xfer.Lang.Scripting;
using ParksComputing.Xfer.Lang.Scripting.Comparison;
using ParksComputing.Xfer.Lang.Scripting.Utility;
using ParksComputing.Xfer.Lang.Scripting.Logical;
using ParksComputing.Xfer.Lang.Elements;
using System.Collections.Generic;

namespace ParksComputing.Xfer.Lang.Tests;

/// <summary>
/// Tests for the ScriptingEngine that orchestrates operator execution and context management.
/// Covers engine initialization, operator registration, execution, and error handling.
/// </summary>
[TestClass]
public class ScriptingEngineTests {

    [TestInitialize]
    public void Setup() {
        // Clear operator registry to ensure clean state
        OperatorRegistry.ClearRegistry();
    }

    [TestCleanup]
    public void Cleanup() {
        // Clear operator registry after each test
        OperatorRegistry.ClearRegistry();
    }

    #region Initialization Tests

    [TestMethod]
    public void ScriptingEngine_Constructor_InitializesCorrectly() {
        // Arrange
        var context = new ScriptingContext();

        // Act
        var engine = new ScriptingEngine(context);

        // Assert
        Assert.IsNotNull(engine);
        Assert.IsNotNull(engine.Context);
        Assert.AreSame(context, engine.Context);
    }

    [TestMethod]
    public void ScriptingEngine_Constructor_NullContext_ThrowsException() {
        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => new ScriptingEngine(null!));
    }

    [TestMethod]
    public void ScriptingEngine_Constructor_RegistersBuiltInOperators() {
        // Arrange
        OperatorRegistry.RegisterBuiltInOperators(); // Ensure registry is populated
        var context = new ScriptingContext();

        // Act
        var engine = new ScriptingEngine(context);

        // Assert
        Assert.IsTrue(engine.IsOperatorRegistered("eq"));
        Assert.IsTrue(engine.IsOperatorRegistered("defined"));
        Assert.IsTrue(engine.IsOperatorRegistered("if"));
    }

    #endregion

    #region Context Management Tests

    [TestMethod]
    public void ScriptingEngine_Context_StartsWithCleanState() {
        // Arrange
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // Assert
        Assert.IsNotNull(engine.Context);
        Assert.IsNotNull(engine.Context.Variables);
        Assert.AreEqual(0, engine.Context.Variables.Count);
    }

    [TestMethod]
    public void ScriptingEngine_Context_CanSetVariable() {
        // Arrange
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // Act
        engine.Context.Variables["TEST_VAR"] = "test_value";

        // Assert
        Assert.AreEqual("test_value", engine.Context.Variables["TEST_VAR"]);
    }

    [TestMethod]
    public void ScriptingEngine_Context_CanClearVariables() {
        // Arrange
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);
        engine.Context.Variables["VAR1"] = "value1";
        engine.Context.Variables["VAR2"] = "value2";

        // Act
        engine.Context.Variables.Clear();

        // Assert
        Assert.AreEqual(0, engine.Context.Variables.Count);
    }

    #endregion

    #region Operator Registration Tests

    [TestMethod]
    public void ScriptingEngine_RegisterOperator_AddsOperatorToEngine() {
        // Arrange
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);
        // Unregister the built-in operator first so we can test registration
        engine.UnregisterOperator("eq");
        var customOperator = new EqualsOperator();

        // Act
        engine.RegisterOperator(customOperator);

        // Assert
        Assert.IsTrue(engine.IsOperatorRegistered("eq"));
        Assert.IsTrue(engine.RegisteredOperators.Contains("eq"));
    }

    [TestMethod]
    public void ScriptingEngine_RegisterOperator_NullOperator_ThrowsException() {
        // Arrange
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => engine.RegisterOperator(null!));
    }

    [TestMethod]
    public void ScriptingEngine_RegisterOperator_DuplicateOperator_ThrowsException() {
        // Arrange
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);
        // eq is already registered by constructor, so registering another should throw
        var operator2 = new EqualsOperator();

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => engine.RegisterOperator(operator2));
    }

    [TestMethod]
    public void ScriptingEngine_UnregisterOperator_RemovesOperator() {
        // Arrange
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);
        // eq is already registered by constructor

        // Act
        var result = engine.UnregisterOperator("eq");

        // Assert
        Assert.IsTrue(result);
        Assert.IsFalse(engine.IsOperatorRegistered("eq"));
    }

    [TestMethod]
    public void ScriptingEngine_UnregisterOperator_NonExistentOperator_ReturnsFalse() {
        // Arrange
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // Act
        var result = engine.UnregisterOperator("nonexistent");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ScriptingEngine_UnregisterOperator_NullOrEmptyName_ReturnsFalse() {
        // Arrange
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // Act & Assert
        Assert.IsFalse(engine.UnregisterOperator(null!));
        Assert.IsFalse(engine.UnregisterOperator(""));
        Assert.IsFalse(engine.UnregisterOperator("   "));
    }

    [TestMethod]
    public void ScriptingEngine_IsOperatorRegistered_RegisteredOperator_ReturnsTrue() {
        // Arrange
        OperatorRegistry.RegisterBuiltInOperators();
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // Act & Assert
        Assert.IsTrue(engine.IsOperatorRegistered("eq"));
        Assert.IsTrue(engine.IsOperatorRegistered("defined"));
        Assert.IsTrue(engine.IsOperatorRegistered("if"));
    }

    [TestMethod]
    public void ScriptingEngine_IsOperatorRegistered_UnregisteredOperator_ReturnsFalse() {
        // Arrange
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // Act & Assert
        Assert.IsFalse(engine.IsOperatorRegistered("nonexistent"));
    }

    [TestMethod]
    public void ScriptingEngine_IsOperatorRegistered_NullOrEmptyName_ReturnsFalse() {
        // Arrange
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // Act & Assert
        Assert.IsFalse(engine.IsOperatorRegistered(null!));
        Assert.IsFalse(engine.IsOperatorRegistered(""));
    }

    #endregion

    #region Operator Execution Tests

    [TestMethod]
    public void ScriptingEngine_Evaluate_EqualsOperator_ReturnsCorrectResult() {
        // Arrange
        OperatorRegistry.RegisterBuiltInOperators();
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);
        var arg1 = new StringElement("hello");
        var arg2 = new StringElement("hello");

        // Act
        var result = engine.Evaluate("eq", arg1, arg2);

        // Assert
        Assert.IsTrue((bool)result!);
    }

    [TestMethod]
    public void ScriptingEngine_Evaluate_DefinedOperator_ReturnsCorrectResult() {
        // Arrange
        OperatorRegistry.RegisterBuiltInOperators();
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);
        var arg = new StringElement("test");

        // Act
        var result = engine.Evaluate("defined", arg);

        // Assert
        Assert.IsTrue((bool)result!);
    }

    [TestMethod]
    public void ScriptingEngine_Evaluate_IfOperator_ReturnsCorrectResult() {
        // Arrange
        OperatorRegistry.RegisterBuiltInOperators();
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);
        var condition = new BooleanElement(true);
        var thenBranch = new StringElement("success");
        var elseBranch = new StringElement("failure");

        // Act
        var result = engine.Evaluate("if", condition, thenBranch, elseBranch);

        // Assert
        Assert.AreEqual("success", (string)result!);
    }

    [TestMethod]
    public void ScriptingEngine_Evaluate_UnregisteredOperator_ThrowsException() {
        // Arrange
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);
        var arg = new StringElement("test");

        // Act & Assert
        var ex = Assert.ThrowsException<ArgumentException>(() => engine.Evaluate("nonexistent", arg));
        Assert.IsTrue(ex.Message.Contains("not registered"));
    }

    [TestMethod]
    public void ScriptingEngine_Evaluate_NullOrEmptyOperatorName_ThrowsException() {
        // Arrange
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);
        var arg = new StringElement("test");

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => engine.Evaluate(null!, arg));
        Assert.ThrowsException<ArgumentException>(() => engine.Evaluate("", arg));
    }

    [TestMethod]
    public void ScriptingEngine_Evaluate_NullArguments_HandlesGracefully() {
        // Arrange
        OperatorRegistry.RegisterBuiltInOperators();
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // Act & Assert - null arguments are converted to empty array, which should fail validation
        Assert.ThrowsException<ArgumentException>(() => engine.Evaluate("defined", null!));
    }

    #endregion

    #region TryEvaluate Tests

    [TestMethod]
    public void ScriptingEngine_TryEvaluate_ValidOperation_ReturnsTrue() {
        // Arrange
        OperatorRegistry.RegisterBuiltInOperators();
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);
        var arg1 = new StringElement("hello");
        var arg2 = new StringElement("hello");

        // Act
        var success = engine.TryEvaluate("eq", new[] { arg1, arg2 }, out var result);

        // Assert
        Assert.IsTrue(success);
        Assert.IsTrue((bool)result!);
    }

    [TestMethod]
    public void ScriptingEngine_TryEvaluate_InvalidOperation_ReturnsFalse() {
        // Arrange
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);
        var arg = new StringElement("test");

        // Act
        var success = engine.TryEvaluate("nonexistent", new[] { arg }, out var result);

        // Assert
        Assert.IsFalse(success);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void ScriptingEngine_TryEvaluate_NullOperatorName_ReturnsFalse() {
        // Arrange
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);
        var arg = new StringElement("test");

        // Act
        var success = engine.TryEvaluate(null!, new[] { arg }, out var result);

        // Assert
        Assert.IsFalse(success);
        Assert.IsNull(result);
    }

    #endregion

    #region Operator Information Tests

    [TestMethod]
    public void ScriptingEngine_GetOperator_RegisteredOperator_ReturnsOperator() {
        // Arrange
        OperatorRegistry.RegisterBuiltInOperators();
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // Act
        var equalsOp = engine.GetOperator("eq");
        var definedOp = engine.GetOperator("defined");
        var ifOp = engine.GetOperator("if");

        // Assert
        Assert.IsNotNull(equalsOp);
        Assert.IsInstanceOfType(equalsOp, typeof(EqualsOperator));
        Assert.IsNotNull(definedOp);
        Assert.IsInstanceOfType(definedOp, typeof(DefinedOperator));
        Assert.IsNotNull(ifOp);
        Assert.IsInstanceOfType(ifOp, typeof(IfOperator));
    }

    [TestMethod]
    public void ScriptingEngine_GetOperator_UnregisteredOperator_ReturnsNull() {
        // Arrange
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // Act
        var result = engine.GetOperator("nonexistent");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void ScriptingEngine_GetOperator_NullOrEmptyName_ReturnsNull() {
        // Arrange
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // Act & Assert
        Assert.IsNull(engine.GetOperator(null!));
        Assert.IsNull(engine.GetOperator(""));
    }

    [TestMethod]
    public void ScriptingEngine_GetAllOperators_ReturnsAllRegisteredOperators() {
        // Arrange
        OperatorRegistry.RegisterBuiltInOperators();
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // Act
        var operators = engine.GetAllOperators().ToList();

        // Assert
        Assert.AreEqual(3, operators.Count);
        Assert.IsTrue(operators.Any(op => op is EqualsOperator));
        Assert.IsTrue(operators.Any(op => op is DefinedOperator));
        Assert.IsTrue(operators.Any(op => op is IfOperator));
    }

    [TestMethod]
    public void ScriptingEngine_GetOperatorsByCategory_ReturnsFilteredOperators() {
        // Arrange
        OperatorRegistry.RegisterBuiltInOperators();
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // Act
        var comparisonOps = engine.GetOperatorsByCategory("Comparison").ToList();
        var utilityOps = engine.GetOperatorsByCategory("Utility").ToList();
        var logicalOps = engine.GetOperatorsByCategory("Logical").ToList();

        // Assert
        Assert.AreEqual(1, comparisonOps.Count);
        Assert.IsInstanceOfType(comparisonOps[0], typeof(EqualsOperator));

        Assert.AreEqual(1, utilityOps.Count);
        Assert.IsInstanceOfType(utilityOps[0], typeof(DefinedOperator));

        Assert.AreEqual(1, logicalOps.Count);
        Assert.IsInstanceOfType(logicalOps[0], typeof(IfOperator));
    }

    [TestMethod]
    public void ScriptingEngine_GetOperatorsByCategory_InvalidCategory_ReturnsEmpty() {
        // Arrange
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // Act
        var result = engine.GetOperatorsByCategory("NonExistent").ToList();

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void ScriptingEngine_GetOperatorsByCategory_NullOrEmptyCategory_ReturnsEmpty() {
        // Arrange
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // Act & Assert
        Assert.AreEqual(0, engine.GetOperatorsByCategory(null!).Count());
        Assert.AreEqual(0, engine.GetOperatorsByCategory("").Count());
    }

    #endregion

    #region Clear Operations Tests

    [TestMethod]
    public void ScriptingEngine_ClearCustomOperators_PreservesBuiltInOperators() {
        // Arrange
        OperatorRegistry.RegisterBuiltInOperators();
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // Register a custom operator
        var customOp = new TestCustomOperator();
        engine.RegisterOperator(customOp);

        var initialCount = engine.RegisteredOperators.Count();

        // Act
        engine.ClearCustomOperators();

        // Assert
        // Built-in operators should still be there
        Assert.IsTrue(engine.IsOperatorRegistered("eq"));
        Assert.IsTrue(engine.IsOperatorRegistered("defined"));
        Assert.IsTrue(engine.IsOperatorRegistered("if"));

        // Custom operator should be removed
        Assert.IsFalse(engine.IsOperatorRegistered("test"));
    }

    #endregion

    #region Diagnostics Tests

    [TestMethod]
    public void ScriptingEngine_GetDiagnosticInfo_ReturnsUsefulInformation() {
        // Arrange
        OperatorRegistry.RegisterBuiltInOperators();
        var context = new ScriptingContext();
        context.Variables["TEST_VAR"] = "test_value";
        var engine = new ScriptingEngine(context);

        // Act
        var diagnostics = engine.GetDiagnosticInfo();

        // Assert
        Assert.IsNotNull(diagnostics);
        Assert.IsTrue(diagnostics.ContainsKey("RegisteredOperatorCount"));
        Assert.IsTrue(diagnostics.ContainsKey("RegisteredOperators"));
        Assert.IsTrue(diagnostics.ContainsKey("ContextVariableCount"));
        Assert.IsTrue(diagnostics.ContainsKey("ContextVariables"));
        Assert.IsTrue(diagnostics.ContainsKey("Environment"));

        Assert.AreEqual(3, (int)diagnostics["RegisteredOperatorCount"]);
        Assert.AreEqual(1, (int)diagnostics["ContextVariableCount"]);

        var operators = (List<string>)diagnostics["RegisteredOperators"];
        Assert.IsTrue(operators.Contains("eq"));
        Assert.IsTrue(operators.Contains("defined"));
        Assert.IsTrue(operators.Contains("if"));

        var variables = (List<string>)diagnostics["ContextVariables"];
        Assert.IsTrue(variables.Contains("TEST_VAR"));
    }

    [TestMethod]
    public void ScriptingEngine_ToString_ReturnsDescriptiveString() {
        // Arrange
        OperatorRegistry.RegisterBuiltInOperators();
        var context = new ScriptingContext();
        context.Variables["TEST_VAR"] = "test_value";
        var engine = new ScriptingEngine(context);

        // Act
        var result = engine.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("3 operators"));
        Assert.IsTrue(result.Contains("1 context variables"));
    }

    #endregion

    #region Multiple Engine Tests

    [TestMethod]
    public void ScriptingEngine_MultipleInstances_IndependentContexts() {
        // Arrange
        var context1 = new ScriptingContext();
        var context2 = new ScriptingContext();
        var engine1 = new ScriptingEngine(context1);
        var engine2 = new ScriptingEngine(context2);

        engine1.Context.Variables["VAR"] = "engine1_value";
        engine2.Context.Variables["VAR"] = "engine2_value";

        // Act & Assert
        Assert.AreEqual("engine1_value", engine1.Context.Variables["VAR"]);
        Assert.AreEqual("engine2_value", engine2.Context.Variables["VAR"]);
        Assert.AreNotEqual(engine1.Context.Variables["VAR"], engine2.Context.Variables["VAR"]);
    }

    #endregion

    #region Complex Expression Tests

    [TestMethod]
    public void ScriptingEngine_ComplexExpression_NestedOperators_ExecutesCorrectly() {
        // Arrange
        OperatorRegistry.RegisterBuiltInOperators();
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);

        // First check if "hello" equals "hello"
        var equalsResult = engine.Evaluate("eq",
            new StringElement("hello"),
            new StringElement("hello"));

        // Then use that result in an if statement
        var ifResult = engine.Evaluate("if",
            new BooleanElement((bool)equalsResult!),
            new StringElement("strings_match"),
            new StringElement("strings_differ"));

        // Assert
        Assert.IsTrue((bool)equalsResult);
        Assert.AreEqual("strings_match", (string)ifResult!);
    }

    [TestMethod]
    public void ScriptingEngine_ComplexExpression_WithVariables_ExecutesCorrectly() {
        // Arrange
        OperatorRegistry.RegisterBuiltInOperators();
        var context = new ScriptingContext();
        var engine = new ScriptingEngine(context);
        engine.Context.Variables["USER_NAME"] = "admin";

        // Create a dynamic element that would resolve from context
        var userNameElement = new StringElement("admin"); // Simulating resolved dynamic element

        // Check if the user name is defined
        var definedResult = engine.Evaluate("defined", userNameElement);

        // Use defined result in conditional logic
        var authResult = engine.Evaluate("if",
            new BooleanElement((bool)definedResult!),
            new StringElement("authenticated"),
            new StringElement("anonymous"));

        // Assert
        Assert.IsTrue((bool)definedResult);
        Assert.AreEqual("authenticated", (string)authResult!);
    }

    #endregion
}

/// <summary>
/// Test custom operator for testing custom operator registration and clearing.
/// </summary>
public class TestCustomOperator : ScriptingOperator {
    public override string OperatorName => "test";
    public override string Description => "Test operator for unit tests";
    public override int MinArguments => 0;
    public override int MaxArguments => 1;

    public override object? Evaluate(ScriptingContext context, params Element[] arguments) {
        return "test_result";
    }
}
