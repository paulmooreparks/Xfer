using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Elements;
using System;

namespace ParksComputing.Xfer.Lang.Tests;

/// <summary>
/// Comprehensive tests for LongElement functionality.
/// Tests 64-bit long integer handling, custom formatting, and serialization behavior.
/// </summary>
[TestClass]
public class LongElementTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_BasicValue_SetsCorrectly()
    {
        // Arrange & Act
        var element = new LongElement(42L);

        // Assert
        Assert.AreEqual(42L, element.Value);
        Assert.AreEqual("longInteger", LongElement.ElementName);
        Assert.AreEqual('&', element.Delimiter.OpeningSpecifier);
        Assert.AreEqual('&', element.Delimiter.ClosingSpecifier);
        Assert.AreEqual(ElementStyle.Compact, element.Delimiter.Style);
        Assert.AreEqual(1, element.Delimiter.SpecifierCount);
    // Custom formatter support removed
    }

    [TestMethod]
    public void Constructor_MinValue_SetsCorrectly()
    {
        // Arrange & Act
        var element = new LongElement(long.MinValue);

        // Assert
        Assert.AreEqual(long.MinValue, element.Value);
    }

    [TestMethod]
    public void Constructor_MaxValue_SetsCorrectly()
    {
        // Arrange & Act
        var element = new LongElement(long.MaxValue);

        // Assert
        Assert.AreEqual(long.MaxValue, element.Value);
    }

    [TestMethod]
    public void Constructor_Zero_SetsCorrectly()
    {
        // Arrange & Act
        var element = new LongElement(0L);

        // Assert
        Assert.AreEqual(0L, element.Value);
    }

    [TestMethod]
    public void Constructor_NegativeValue_SetsCorrectly()
    {
        // Arrange & Act
        var element = new LongElement(-1000L);

        // Assert
        Assert.AreEqual(-1000L, element.Value);
    }

    [TestMethod]
    public void Constructor_CustomSpecifierCount_SetsCorrectly()
    {
        // Arrange & Act
        var element = new LongElement(42L, specifierCount: 3);

        // Assert
        Assert.AreEqual(3, element.Delimiter.SpecifierCount);
    }

    [TestMethod]
    public void Constructor_ExplicitStyle_SetsCorrectly()
    {
        // Arrange & Act
        var element = new LongElement(42L, style: ElementStyle.Explicit);

        // Assert
        Assert.AreEqual(ElementStyle.Explicit, element.Delimiter.Style);
    }

    [TestMethod]
    public void Constructor_ImplicitStyle_SetsCorrectly()
    {
        // Arrange & Act
        var element = new LongElement(42L, style: ElementStyle.Implicit);

        // Assert
        Assert.AreEqual(ElementStyle.Implicit, element.Delimiter.Style);
    }

    // Removed custom formatter constructor test

    #endregion

    #region Inheritance Tests

    [TestMethod]
    public void Inheritance_IsNumericElement()
    {
        // Arrange & Act
        var element = new LongElement(42L);

        // Assert
        Assert.IsInstanceOfType(element, typeof(NumericElement<long>));
        Assert.IsInstanceOfType(element, typeof(TypedElement<long>));
    }

    [TestMethod]
    public void Inheritance_HasNumericElementProperties()
    {
        // Arrange
        var value = 42L;
        var element = new LongElement(value);

        // Act & Assert
        Assert.AreEqual(value, element.Value);
        Assert.AreEqual(value.ToString(), element.ToString());
    }

    #endregion

    #region Serialization Tests - Default Formatting

    [TestMethod]
    public void ToXfer_CompactStyle_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new LongElement(42L); // Default compact style

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("&42", result);
    }

    [TestMethod]
    public void ToXfer_ExplicitStyle_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new LongElement(42L, style: ElementStyle.Explicit);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("<&42&>", result);
    }

    [TestMethod]
    public void ToXfer_ZeroValue_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new LongElement(0L);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("&0", result);
    }

    [TestMethod]
    public void ToXfer_NegativeValue_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new LongElement(-42L);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("&-42", result);
    }

    [TestMethod]
    public void ToXfer_LargeValue_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new LongElement(9223372036854775807L); // long.MaxValue

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("&9223372036854775807", result);
    }

    [TestMethod]
    public void ToXfer_VeryNegativeValue_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new LongElement(-9223372036854775808L); // long.MinValue

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("&-9223372036854775808", result);
    }

    #endregion

    // Removed custom formatting serialization tests

    #region Serialization Tests - Multiple Specifiers

    [TestMethod]
    public void ToXfer_MultipleSpecifiers_CompactStyle_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new LongElement(42L, specifierCount: 3);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("&&&42", result);
    }

    [TestMethod]
    public void ToXfer_MultipleSpecifiers_ExplicitStyle_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new LongElement(42L, specifierCount: 2, style: ElementStyle.Explicit);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("<&&42&&>", result);
    }

    #endregion

    // Removed CustomFormatter region

    #region ToString Tests

    [TestMethod]
    public void ToString_ReturnsStringValue()
    {
        var testValues = new[]
        {
            0L,
            42L,
            -42L,
            long.MinValue,
            long.MaxValue,
            1000000000000L,
        };

        foreach (var value in testValues)
        {
            var element = new LongElement(value);
            Assert.AreEqual(value.ToString(), element.ToString());
        }
    }

    // Removed ToString with custom formatter test

    #endregion

    #region Edge Cases

    [TestMethod]
    public void EdgeCases_ExtremeValues_HandleCorrectly()
    {
        var extremeValues = new[]
        {
            long.MinValue,      // -9,223,372,036,854,775,808
            long.MaxValue,      // 9,223,372,036,854,775,807
            0L,
            1L,
            -1L,
        };

        foreach (var value in extremeValues)
        {
            var element = new LongElement(value);

            Assert.AreEqual(value, element.Value);

            var xfer = element.ToXfer();
            Assert.IsTrue(xfer.Contains(value.ToString()));

            var str = element.ToString();
            Assert.AreEqual(value.ToString(), str);
        }
    }

    [TestMethod]
    public void EdgeCases_CommonLargeNumbers_HandleCorrectly()
    {
        var largeNumbers = new[]
        {
            1000L,              // Thousand
            1000000L,           // Million
            1000000000L,        // Billion
            1000000000000L,     // Trillion
            1000000000000000L,  // Quadrillion
        };

        foreach (var value in largeNumbers)
        {
            var element = new LongElement(value);
            Assert.AreEqual(value, element.Value);
        }
    }

    #endregion

    #region Formatting With Parameters Tests

    [TestMethod]
    public void ToXfer_WithFormattingParameters_IgnoresParameters()
    {
        // Arrange
        var element = new LongElement(42L);

        // Act
        var result = element.ToXfer(Formatting.Indented, indentChar: '\t', indentation: 4, depth: 2);

        // Assert
        Assert.AreEqual("&42", result); // Numeric elements typically ignore formatting parameters
    }

    #endregion

    #region Element Delimiter Tests

    [TestMethod]
    public void ElementDelimiter_Properties_SetCorrectly()
    {
        // Arrange
        var element = new LongElement(42L);

        // Act & Assert
        Assert.AreEqual('&', element.Delimiter.OpeningSpecifier);
        Assert.AreEqual('&', element.Delimiter.ClosingSpecifier);
        Assert.AreEqual(ElementStyle.Compact, element.Delimiter.Style);
        Assert.AreEqual(1, element.Delimiter.SpecifierCount);
    }

    [TestMethod]
    public void ElementDelimiter_StaticProperty_MatchesInstance()
    {
        // Arrange
        var element = new LongElement(42L);

        // Act & Assert
        Assert.AreEqual(LongElement.ElementDelimiter.OpeningSpecifier, element.Delimiter.OpeningSpecifier);
        Assert.AreEqual(LongElement.ElementDelimiter.ClosingSpecifier, element.Delimiter.ClosingSpecifier);
    }

    #endregion

    #region Realistic Use Cases

    [TestMethod]
    public void RealisticUseCases_Timestamps_HandleCorrectly()
    {
        // Unix timestamps
        var timestamps = new[]
        {
            0L,                 // Unix epoch
            946684800L,         // Year 2000
            1609459200L,        // Year 2021
            1640995200L,        // Year 2022
            2147483647L,        // Year 2038 problem
        };

        foreach (var timestamp in timestamps)
        {
            var element = new LongElement(timestamp);
            Assert.AreEqual(timestamp, element.Value);
        }
    }

    [TestMethod]
    public void RealisticUseCases_MemorySizes_HandleCorrectly()
    {
        // Memory sizes in bytes
        var memorySizes = new[]
        {
            1024L,              // 1 KB
            1048576L,           // 1 MB
            1073741824L,        // 1 GB
            1099511627776L,     // 1 TB
        };

        foreach (var size in memorySizes)
        {
            var element = new LongElement(size);
            Assert.AreEqual(size, element.Value);

            // Hex formatting now done via attribute infrastructure; custom formatter removed
        }
    }

    #endregion
}
