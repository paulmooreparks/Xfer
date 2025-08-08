using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ParksComputing.Xfer.Lang.Scripting;
using ParksComputing.Xfer.Lang.Scripting.Comparison;
using ParksComputing.Xfer.Lang.Scripting.Utility;
using ParksComputing.Xfer.Lang.Scripting.Logical;

namespace ParksComputing.Xfer.Lang.Tests;

/// <summary>
/// Tests for the OperatorRegistry system that manages scripting operator registration and discovery.
/// Ensures proper thread-safe operation, extensibility, and operator validation.
/// </summary>
[TestClass]
public class OperatorRegistryTests {

    [TestInitialize]
    public void Setup() {
        // Clear the registry before each test to ensure clean state
        OperatorRegistry.ClearRegistry();
    }

    [TestCleanup]
    public void Cleanup() {
        // Clear the registry after each test to avoid side effects
        OperatorRegistry.ClearRegistry();
    }

    #region Registration Tests

    [TestMethod]
    public void RegisterOperator_Generic_RegistersOperatorType() {
        // Act
        OperatorRegistry.RegisterOperator<EqualsOperator>();

        // Assert
        Assert.IsTrue(OperatorRegistry.IsOperatorRegistered("eq"));
        Assert.AreEqual(typeof(EqualsOperator), OperatorRegistry.GetOperatorType("eq"));
    }

    [TestMethod]
    public void RegisterOperator_NonGeneric_RegistersOperatorType() {
        // Act
        OperatorRegistry.RegisterOperator("eq", typeof(EqualsOperator));

        // Assert
        Assert.IsTrue(OperatorRegistry.IsOperatorRegistered("eq"));
        Assert.AreEqual(typeof(EqualsOperator), OperatorRegistry.GetOperatorType("eq"));
    }

    [TestMethod]
    public void RegisterOperator_MultipleOperators_AllRegistered() {
        // Act
        OperatorRegistry.RegisterOperator<EqualsOperator>();
        OperatorRegistry.RegisterOperator<DefinedOperator>();
        OperatorRegistry.RegisterOperator<IfOperator>();

        // Assert
        Assert.IsTrue(OperatorRegistry.IsOperatorRegistered("eq"));
        Assert.IsTrue(OperatorRegistry.IsOperatorRegistered("defined"));
        Assert.IsTrue(OperatorRegistry.IsOperatorRegistered("if"));

        Assert.AreEqual(typeof(EqualsOperator), OperatorRegistry.GetOperatorType("eq"));
        Assert.AreEqual(typeof(DefinedOperator), OperatorRegistry.GetOperatorType("defined"));
        Assert.AreEqual(typeof(IfOperator), OperatorRegistry.GetOperatorType("if"));
    }

    [TestMethod]
    public void RegisterOperator_DuplicateRegistration_OverwritesExisting() {
        // Arrange
        OperatorRegistry.RegisterOperator<EqualsOperator>();
        Assert.IsTrue(OperatorRegistry.IsOperatorRegistered("eq"));

        // Act - Register the same operator again
        OperatorRegistry.RegisterOperator<EqualsOperator>();

        // Assert - Should still be registered
        Assert.IsTrue(OperatorRegistry.IsOperatorRegistered("eq"));
        Assert.AreEqual(typeof(EqualsOperator), OperatorRegistry.GetOperatorType("eq"));
    }

    #endregion

    #region Validation Tests

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void RegisterOperator_NullOperatorName_ThrowsArgumentException() {
        // Act & Assert
        OperatorRegistry.RegisterOperator(null!, typeof(EqualsOperator));
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void RegisterOperator_EmptyOperatorName_ThrowsArgumentException() {
        // Act & Assert
        OperatorRegistry.RegisterOperator("", typeof(EqualsOperator));
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RegisterOperator_NullOperatorType_ThrowsArgumentNullException() {
        // Act & Assert
        OperatorRegistry.RegisterOperator("test", null!);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void RegisterOperator_NonScriptingOperatorType_ThrowsArgumentException() {
        // Act & Assert
        OperatorRegistry.RegisterOperator("invalid", typeof(string));
    }

    #endregion

    #region Lookup Tests

    [TestMethod]
    public void IsOperatorRegistered_UnregisteredOperator_ReturnsFalse() {
        // Act & Assert
        Assert.IsFalse(OperatorRegistry.IsOperatorRegistered("nonexistent"));
        Assert.IsFalse(OperatorRegistry.IsOperatorRegistered("eq"));
    }

    [TestMethod]
    public void IsOperatorRegistered_RegisteredOperator_ReturnsTrue() {
        // Arrange
        OperatorRegistry.RegisterOperator<EqualsOperator>();

        // Act & Assert
        Assert.IsTrue(OperatorRegistry.IsOperatorRegistered("eq"));
    }

    [TestMethod]
    public void IsOperatorRegistered_CaseInsensitive_ReturnsTrue() {
        // Arrange
        OperatorRegistry.RegisterOperator<EqualsOperator>();

        // Act & Assert
        Assert.IsTrue(OperatorRegistry.IsOperatorRegistered("EQ"));
        Assert.IsTrue(OperatorRegistry.IsOperatorRegistered("Eq"));
        Assert.IsTrue(OperatorRegistry.IsOperatorRegistered("eQ"));
    }

    [TestMethod]
    public void GetOperatorType_UnregisteredOperator_ReturnsNull() {
        // Act & Assert
        Assert.IsNull(OperatorRegistry.GetOperatorType("nonexistent"));
    }

    [TestMethod]
    public void GetOperatorType_RegisteredOperator_ReturnsCorrectType() {
        // Arrange
        OperatorRegistry.RegisterOperator<EqualsOperator>();

        // Act & Assert
        Assert.AreEqual(typeof(EqualsOperator), OperatorRegistry.GetOperatorType("eq"));
    }

    [TestMethod]
    public void GetOperatorType_CaseInsensitive_ReturnsCorrectType() {
        // Arrange
        OperatorRegistry.RegisterOperator<EqualsOperator>();

        // Act & Assert
        Assert.AreEqual(typeof(EqualsOperator), OperatorRegistry.GetOperatorType("EQ"));
        Assert.AreEqual(typeof(EqualsOperator), OperatorRegistry.GetOperatorType("Eq"));
    }

    #endregion

    #region Built-in Registration Tests

    [TestMethod]
    public void RegisterBuiltInOperators_RegistersAllBuiltInOperators() {
        // Act
        OperatorRegistry.RegisterBuiltInOperators();

        // Assert - Check that all expected built-in operators are registered
        Assert.IsTrue(OperatorRegistry.IsOperatorRegistered("eq"));
        Assert.IsTrue(OperatorRegistry.IsOperatorRegistered("defined"));
        Assert.IsTrue(OperatorRegistry.IsOperatorRegistered("if"));

        // Verify types are correct
        Assert.AreEqual(typeof(EqualsOperator), OperatorRegistry.GetOperatorType("eq"));
        Assert.AreEqual(typeof(DefinedOperator), OperatorRegistry.GetOperatorType("defined"));
        Assert.AreEqual(typeof(IfOperator), OperatorRegistry.GetOperatorType("if"));
    }

    [TestMethod]
    public void RegisterBuiltInOperators_CalledMultipleTimes_DoesNotThrow() {
        // Act & Assert - Should not throw exceptions when called multiple times
        OperatorRegistry.RegisterBuiltInOperators();
        OperatorRegistry.RegisterBuiltInOperators();
        OperatorRegistry.RegisterBuiltInOperators();

        // Verify operators are still registered correctly
        Assert.IsTrue(OperatorRegistry.IsOperatorRegistered("eq"));
        Assert.IsTrue(OperatorRegistry.IsOperatorRegistered("defined"));
        Assert.IsTrue(OperatorRegistry.IsOperatorRegistered("if"));
    }

    #endregion

    #region Creation Tests

    [TestMethod]
    public void CreateOperator_RegisteredOperator_ReturnsCorrectInstance() {
        // Arrange
        OperatorRegistry.RegisterOperator<EqualsOperator>();

        // Act
        var operatorInstance = OperatorRegistry.CreateOperator("eq");

        // Assert
        Assert.IsNotNull(operatorInstance);
        Assert.IsInstanceOfType(operatorInstance, typeof(EqualsOperator));
        Assert.AreEqual("eq", operatorInstance.OperatorName);
    }

    [TestMethod]
    public void CreateOperator_UnregisteredOperator_ReturnsNull() {
        // Act
        var operatorInstance = OperatorRegistry.CreateOperator("nonexistent");

        // Assert
        Assert.IsNull(operatorInstance);
    }

    [TestMethod]
    public void CreateOperator_CaseInsensitive_ReturnsCorrectInstance() {
        // Arrange
        OperatorRegistry.RegisterOperator<EqualsOperator>();

        // Act
        var operatorInstance = OperatorRegistry.CreateOperator("EQ");

        // Assert
        Assert.IsNotNull(operatorInstance);
        Assert.IsInstanceOfType(operatorInstance, typeof(EqualsOperator));
    }

    #endregion

    #region Enumeration Tests

    [TestMethod]
    public void GetRegisteredOperatorNames_EmptyRegistry_ReturnsEmptyCollection() {
        // Act
        var operatorNames = OperatorRegistry.GetRegisteredOperatorNames();

        // Assert
        Assert.IsNotNull(operatorNames);
        Assert.AreEqual(0, operatorNames.Count);
    }

    [TestMethod]
    public void GetRegisteredOperatorNames_WithRegisteredOperators_ReturnsAllNames() {
        // Arrange
        OperatorRegistry.RegisterOperator<EqualsOperator>();
        OperatorRegistry.RegisterOperator<DefinedOperator>();

        // Act
        var operatorNames = OperatorRegistry.GetRegisteredOperatorNames();

        // Assert
        Assert.IsNotNull(operatorNames);
        Assert.AreEqual(2, operatorNames.Count);
        Assert.IsTrue(operatorNames.Contains("eq"));
        Assert.IsTrue(operatorNames.Contains("defined"));
    }

    [TestMethod]
    public void GetRegisteredOperatorNames_ReturnsReadOnlyCollection() {
        // Arrange
        OperatorRegistry.RegisterOperator<EqualsOperator>();

        // Act
        var operatorNames = OperatorRegistry.GetRegisteredOperatorNames();

        // Assert
        Assert.IsNotNull(operatorNames);
        Assert.IsTrue(operatorNames.Contains("eq"));
    }

    #endregion

    #region Thread Safety Tests

    [TestMethod]
    public void ConcurrentRegistration_MultipleThreads_AllOperatorsRegistered() {
        // Arrange
        var operatorTypes = new[] {
            typeof(EqualsOperator),
            typeof(DefinedOperator),
            typeof(IfOperator)
        };

        var operatorNames = new[] { "eq", "defined", "if" };

        // Act - Register operators concurrently from multiple threads
        System.Threading.Tasks.Parallel.For(0, operatorTypes.Length, i => {
            OperatorRegistry.RegisterOperator(operatorNames[i], operatorTypes[i]);
        });

        // Assert - All operators should be registered
        for (int i = 0; i < operatorNames.Length; i++) {
            Assert.IsTrue(OperatorRegistry.IsOperatorRegistered(operatorNames[i]));
            Assert.AreEqual(operatorTypes[i], OperatorRegistry.GetOperatorType(operatorNames[i]));
        }
    }

    [TestMethod]
    public void ConcurrentLookup_MultipleThreads_AllLookupsSucceed() {
        // Arrange
        OperatorRegistry.RegisterOperator<EqualsOperator>();
        OperatorRegistry.RegisterOperator<DefinedOperator>();

        var results = new bool[100];

        // Act - Perform concurrent lookups from multiple threads
        System.Threading.Tasks.Parallel.For(0, 100, i => {
            results[i] = OperatorRegistry.IsOperatorRegistered("eq") &&
                        OperatorRegistry.IsOperatorRegistered("defined");
        });

        // Assert - All lookups should succeed
        Assert.IsTrue(results.All(result => result));
    }

    #endregion

    #region Clear Registry Tests

    [TestMethod]
    public void ClearRegistry_WithRegisteredOperators_RemovesAllOperators() {
        // Arrange
        OperatorRegistry.RegisterOperator<EqualsOperator>();
        OperatorRegistry.RegisterOperator<DefinedOperator>();
        Assert.IsTrue(OperatorRegistry.IsOperatorRegistered("eq"));
        Assert.IsTrue(OperatorRegistry.IsOperatorRegistered("defined"));

        // Act
        OperatorRegistry.ClearRegistry();

        // Assert
        Assert.IsFalse(OperatorRegistry.IsOperatorRegistered("eq"));
        Assert.IsFalse(OperatorRegistry.IsOperatorRegistered("defined"));
        Assert.AreEqual(0, OperatorRegistry.GetRegisteredOperatorNames().Count);
    }

    [TestMethod]
    public void ClearRegistry_EmptyRegistry_DoesNotThrow() {
        // Act & Assert - Should not throw
        OperatorRegistry.ClearRegistry();

        // Verify it's still empty
        Assert.AreEqual(0, OperatorRegistry.GetRegisteredOperatorNames().Count);
    }

    #endregion
}
