using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Elements;
using System;

namespace ParksComputing.Xfer.Lang.Tests;

/// <summary>
/// Comprehensive tests for NumericElement abstract base class functionality.
/// Tests common numeric element behavior across different numeric types.
/// </summary>
[TestClass]
public class NumericElementTests
{
    #region Test Helper Classes

    // Concrete implementation for testing abstract NumericElement
    private class TestNumericElement : NumericElement<int>
    {
        public static readonly string TestElementName = "testNumeric";
        public static readonly ElementDelimiter TestElementDelimiter = new ElementDelimiter('#', '#');

        public TestNumericElement(int value)
            : base(value, TestElementName, TestElementDelimiter)
        {
        }

        public TestNumericElement(int value, ElementStyle style)
            : base(value, TestElementName, new ElementDelimiter('#', '#', 1, style))
        {
        }

        public TestNumericElement(int value, int specifierCount, ElementStyle style)
            : base(value, TestElementName, new ElementDelimiter('#', '#', specifierCount, style))
        {
        }
    }

    #endregion

    #region Constructor Tests

    [TestMethod]
    public void Constructor_WithValue_SetsCorrectly()
    {
        // Arrange & Act
        var element = new TestNumericElement(42);

        // Assert
        Assert.AreEqual(42, element.Value);
        Assert.AreEqual("testNumeric", element.Name);
        Assert.AreEqual('#', element.Delimiter.OpeningSpecifier);
        Assert.AreEqual('#', element.Delimiter.ClosingSpecifier);
    }

    [TestMethod]
    public void Constructor_WithZeroValue_SetsCorrectly()
    {
        // Arrange & Act
        var element = new TestNumericElement(0);

        // Assert
        Assert.AreEqual(0, element.Value);
    }

    [TestMethod]
    public void Constructor_WithNegativeValue_SetsCorrectly()
    {
        // Arrange & Act
        var element = new TestNumericElement(-1000);

        // Assert
        Assert.AreEqual(-1000, element.Value);
    }

    [TestMethod]
    public void Constructor_WithMaxValue_SetsCorrectly()
    {
        // Arrange & Act
        var element = new TestNumericElement(int.MaxValue);

        // Assert
        Assert.AreEqual(int.MaxValue, element.Value);
    }

    [TestMethod]
    public void Constructor_WithMinValue_SetsCorrectly()
    {
        // Arrange & Act
        var element = new TestNumericElement(int.MinValue);

        // Assert
        Assert.AreEqual(int.MinValue, element.Value);
    }

    #endregion

    #region Inheritance Tests

    [TestMethod]
    public void Inheritance_IsTypedElement()
    {
        // Arrange & Act
        var element = new TestNumericElement(42);

        // Assert
        Assert.IsInstanceOfType(element, typeof(TypedElement<int>));
        Assert.IsInstanceOfType(element, typeof(Element));
    }

    [TestMethod]
    public void Inheritance_HasTypedElementProperties()
    {
        // Arrange
        var value = 42;
        var element = new TestNumericElement(value);

        // Act & Assert
        Assert.AreEqual(value, element.Value);
        Assert.AreEqual(value.ToString(), element.ToString());
    }

    #endregion

    #region Serialization Tests - Default Formatting

    [TestMethod]
    public void ToXfer_ImplicitStyle_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new TestNumericElement(42, ElementStyle.Implicit);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ToXfer_CompactStyle_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new TestNumericElement(42, ElementStyle.Compact);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("#42", result);
    }

    [TestMethod]
    public void ToXfer_ExplicitStyle_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new TestNumericElement(42, ElementStyle.Explicit);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("<#42#>", result);
    }

    [TestMethod]
    public void ToXfer_ZeroValue_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new TestNumericElement(0);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("#0", result);
    }

    [TestMethod]
    public void ToXfer_NegativeValue_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new TestNumericElement(-42);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("#-42", result);
    }

    [TestMethod]
    public void ToXfer_LargeValue_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new TestNumericElement(2147483647); // int.MaxValue

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("#2147483647", result);
    }

    [TestMethod]
    public void ToXfer_VeryNegativeValue_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new TestNumericElement(-2147483648); // int.MinValue

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("#-2147483648", result);
    }

    #endregion

    #region Serialization Tests - Multiple Specifiers

    [TestMethod]
    public void ToXfer_MultipleSpecifiers_CompactStyle_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new TestNumericElement(42, 2, ElementStyle.Compact);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("##42", result);
    }

    [TestMethod]
    public void ToXfer_MultipleSpecifiers_ExplicitStyle_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new TestNumericElement(42, 3, ElementStyle.Explicit);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("<###42###>", result);
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_ReturnsStringValue()
    {
        var testValues = new[]
        {
            0,
            42,
            -42,
            int.MinValue,
            int.MaxValue,
            1000000,
        };

        foreach (var value in testValues)
        {
            var element = new TestNumericElement(value);
            Assert.AreEqual(value.ToString(), element.ToString());
        }
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void EdgeCases_ExtremeValues_HandleCorrectly()
    {
        var extremeValues = new[]
        {
            int.MinValue,      // -2,147,483,648
            int.MaxValue,      // 2,147,483,647
            0,
            1,
            -1,
        };

        foreach (var value in extremeValues)
        {
            var element = new TestNumericElement(value);

            Assert.AreEqual(value, element.Value);

            var xfer = element.ToXfer();
            Assert.IsTrue(xfer.Contains(value.ToString()));

            var str = element.ToString();
            Assert.AreEqual(value.ToString(), str);
        }
    }

    [TestMethod]
    public void EdgeCases_PowersOfTwo_HandleCorrectly()
    {
        var powersOfTwo = new[]
        {
            1,
            2,
            4,
            8,
            16,
            32,
            64,
            128,
            256,
            512,
            1024,
            1048576,       // 2^20
            1073741824,    // 2^30
        };

        foreach (var value in powersOfTwo)
        {
            var element = new TestNumericElement(value);
            Assert.AreEqual(value, element.Value);
        }
    }

    [TestMethod]
    public void EdgeCases_CommonLargeNumbers_HandleCorrectly()
    {
        var largeNumbers = new[]
        {
            1000,              // Thousand
            1000000,           // Million
            1000000000,        // Billion (still within int range)
        };

        foreach (var value in largeNumbers)
        {
            var element = new TestNumericElement(value);
            Assert.AreEqual(value, element.Value);
        }
    }

    #endregion

    #region Formatting With Parameters Tests

    [TestMethod]
    public void ToXfer_WithFormattingParameters_IgnoresParameters()
    {
        // Arrange
        var element = new TestNumericElement(42);

        // Act
        var result = element.ToXfer(Formatting.Indented, indentChar: '\t', indentation: 4, depth: 2);

        // Assert
        Assert.AreEqual("#42", result); // Numeric elements typically ignore formatting parameters
    }

    #endregion

    #region Element Delimiter Tests

    [TestMethod]
    public void ElementDelimiter_Properties_SetCorrectly()
    {
        // Arrange
        var element = new TestNumericElement(42);

        // Act & Assert
        Assert.AreEqual('#', element.Delimiter.OpeningSpecifier);
        Assert.AreEqual('#', element.Delimiter.ClosingSpecifier);
        Assert.AreEqual(ElementStyle.Compact, element.Delimiter.Style);
        Assert.AreEqual(1, element.Delimiter.SpecifierCount);
    }

    [TestMethod]
    public void ElementDelimiter_StaticProperty_MatchesInstance()
    {
        // Arrange
        var element = new TestNumericElement(42);

        // Act & Assert
        Assert.AreEqual(TestNumericElement.TestElementDelimiter.OpeningSpecifier, element.Delimiter.OpeningSpecifier);
        Assert.AreEqual(TestNumericElement.TestElementDelimiter.ClosingSpecifier, element.Delimiter.ClosingSpecifier);
    }

    #endregion

    #region Realistic Use Cases

    [TestMethod]
    public void RealisticUseCases_CommonNumbers_HandleCorrectly()
    {
        // Common numbers found in applications
        var commonNumbers = new[]
        {
            0,                 // Default/empty
            1,                 // Unity/true
            -1,                // Error code
            100,               // Percentage
            404,               // HTTP error
            1024,              // Computer memory
        };

        foreach (var number in commonNumbers)
        {
            var element = new TestNumericElement(number);
            Assert.AreEqual(number, element.Value);
        }
    }

    [TestMethod]
    public void RealisticUseCases_ConfigurationValues_HandleCorrectly()
    {
        // Common configuration values
        var configValues = new[]
        {
            8080,              // Port number
            3600,              // Seconds in hour
            86400,             // Seconds in day
            65536,             // 64KB
        };

        foreach (var value in configValues)
        {
            var element = new TestNumericElement(value);
            Assert.AreEqual(value, element.Value);

            var xfer = element.ToXfer();
            Assert.IsTrue(xfer.StartsWith("#"));
        }
    }

    #endregion

    #region Value Property Tests

    [TestMethod]
    public void Value_GetAndSet_WorksCorrectly()
    {
        // Arrange
        var element = new TestNumericElement(0);

        // Act
        element.Value = 123;

        // Assert
        Assert.AreEqual(123, element.Value);
    }

    [TestMethod]
    public void Value_SetToExtreme_WorksCorrectly()
    {
        // Arrange
        var element = new TestNumericElement(0);

        // Act & Assert - Min Value
        element.Value = int.MinValue;
        Assert.AreEqual(int.MinValue, element.Value);

        // Act & Assert - Max Value
        element.Value = int.MaxValue;
        Assert.AreEqual(int.MaxValue, element.Value);
    }

    #endregion
}
