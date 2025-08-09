using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Elements;
using System;

namespace ParksComputing.Xfer.Lang.Tests;

/// <summary>
/// Comprehensive tests for CharacterElement functionality.
/// Tests Unicode code point storage, serialization, and delimiter behavior.
/// </summary>
[TestClass]
public class CharacterElementTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_ValidCodePoint_SetsValue()
    {
        // Arrange & Act
        var element = new CharacterElement(65); // 'A'

        // Assert
        Assert.AreEqual(65, element.Value);
        Assert.AreEqual("character", CharacterElement.ElementName);
        Assert.AreEqual('\\', element.Delimiter.OpeningSpecifier);
        Assert.AreEqual('\\', element.Delimiter.ClosingSpecifier);
    }

    [TestMethod]
    public void Constructor_MinValidCodePoint_SetsValue()
    {
        // Arrange & Act
        var element = new CharacterElement(0); // Null character

        // Assert
        Assert.AreEqual(0, element.Value);
    }

    [TestMethod]
    public void Constructor_MaxValidCodePoint_SetsValue()
    {
        // Arrange & Act
        var element = new CharacterElement(0x10FFFF); // Maximum Unicode code point

        // Assert
        Assert.AreEqual(0x10FFFF, element.Value);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Constructor_NegativeCodePoint_ThrowsException()
    {
        // Arrange & Act
        new CharacterElement(-1);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Constructor_CodePointTooLarge_ThrowsException()
    {
        // Arrange & Act
        new CharacterElement(0x110000); // Beyond maximum Unicode code point
    }

    [TestMethod]
    public void Constructor_CustomSpecifierCount_SetsCorrectly()
    {
        // Arrange & Act
        var element = new CharacterElement(65, specifierCount: 3);

        // Assert
        Assert.AreEqual(3, element.Delimiter.SpecifierCount);
    }

    [TestMethod]
    public void Constructor_ExplicitStyle_SetsCorrectly()
    {
        // Arrange & Act
        var element = new CharacterElement(65, style: ElementStyle.Explicit);

        // Assert
        Assert.AreEqual(ElementStyle.Explicit, element.Delimiter.Style);
    }

    #endregion

    #region Serialization Tests

    [TestMethod]
    public void ToXfer_BasicCharacter_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new CharacterElement(65); // 'A'

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("\\$41", result);
    }

    [TestMethod]
    public void ToXfer_NullCharacter_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new CharacterElement(0);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("\\$0 ", result);
    }

    [TestMethod]
    public void ToXfer_HighCodePoint_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new CharacterElement(0x1F600); // Emoji grinning face

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("\\$1F600 ", result);
    }

    [TestMethod]
    public void ToXfer_MaxCodePoint_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new CharacterElement(0x10FFFF);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual("\\$10FFFF ", result);
    }

    [TestMethod]
    public void ToXfer_WithFormatting_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new CharacterElement(65);

        // Act
        var result = element.ToXfer(Formatting.Indented, indentation: 4, depth: 1);

        // Assert
        Assert.AreEqual("\\$41 ", result); // Formatting doesn't affect character elements
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_BasicCharacter_ReturnsCharacter()
    {
        // Arrange
        var element = new CharacterElement(65); // 'A'

        // Act
        var result = element.ToString();

        // Assert
        Assert.AreEqual("A", result);
    }

    [TestMethod]
    public void ToString_Space_ReturnsSpace()
    {
        // Arrange
        var element = new CharacterElement(32); // Space

        // Act
        var result = element.ToString();

        // Assert
        Assert.AreEqual(" ", result);
    }

    [TestMethod]
    public void ToString_EmojiCharacter_ReturnsEmoji()
    {
        // Arrange
        var element = new CharacterElement(0x1F600); // Grinning face emoji

        // Act
        var result = element.ToString();

        // Assert
        Assert.AreEqual("😀", result);
    }

    [TestMethod]
    public void ToString_NewlineCharacter_ReturnsNewline()
    {
        // Arrange
        var element = new CharacterElement(10); // Newline

        // Act
        var result = element.ToString();

        // Assert
        Assert.AreEqual("\n", result);
    }

    [TestMethod]
    public void ToString_SurrogateCharacter_ReturnsCorrectly()
    {
        // Arrange
        var element = new CharacterElement(0x1D11E); // Musical symbol G clef

        // Act
        var result = element.ToString();

        // Assert
        Assert.AreEqual("𝄞", result);
    }

    #endregion

    #region Common Character Tests

    [TestMethod]
    public void CommonCharacters_ASCII_WorkCorrectly()
    {
        // Test common ASCII characters
        var testCases = new[]
        {
            (48, "0", "\\$30 "),   // '0'
            (57, "9", "\\$39 "),   // '9'
            (65, "A", "\\$41 "),   // 'A'
            (90, "Z", "\\$5A "),   // 'Z'
            (97, "a", "\\$61 "),   // 'a'
            (122, "z", "\\$7A "),  // 'z'
            (33, "!", "\\$21 "),   // '!'
            (64, "@", "\\$40 "),   // '@'
        };

        foreach (var (codePoint, expectedChar, expectedXfer) in testCases)
        {
            var element = new CharacterElement(codePoint);
            Assert.AreEqual(expectedChar, element.ToString(), $"ToString failed for code point {codePoint}");
            Assert.AreEqual(expectedXfer, element.ToXfer(), $"ToXfer failed for code point {codePoint}");
        }
    }

    [TestMethod]
    public void UnicodeCharacters_VariousScripts_WorkCorrectly()
    {
        // Test various Unicode scripts
        var testCases = new[]
        {
            (0x03B1, "α"),      // Greek small letter alpha
            (0x0448, "ш"),      // Cyrillic small letter sha
            (0x4E2D, "中"),     // CJK unified ideograph (Chinese)
            (0x05D0, "א"),      // Hebrew letter alef
            (0x0627, "ا"),      // Arabic letter alef
        };

        foreach (var (codePoint, expectedChar) in testCases)
        {
            var element = new CharacterElement(codePoint);
            Assert.AreEqual(expectedChar, element.ToString(), $"Unicode character failed for code point U+{codePoint:X4}");
        }
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void EdgeCases_ControlCharacters_HandleCorrectly()
    {
        // Test control characters
        var controlChars = new[] { 0, 1, 7, 8, 9, 10, 13, 27, 127 };

        foreach (var codePoint in controlChars)
        {
            var element = new CharacterElement(codePoint);
            Assert.AreEqual(codePoint, element.Value);

            // ToString should return the actual character (even if it's a control character)
            var result = element.ToString();
            Assert.IsNotNull(result);

            // ToXfer should return hex format
            var xfer = element.ToXfer();
            Assert.IsTrue(xfer.StartsWith("\\$"));
            Assert.IsTrue(xfer.EndsWith(" "));
        }
    }

    [TestMethod]
    public void EdgeCases_HighSurrogateRange_HandleCorrectly()
    {
        // Test characters that require surrogate pairs
        var highCodePoints = new[] { 0x10000, 0x1F300, 0x20000, 0x10FFFF };

        foreach (var codePoint in highCodePoints)
        {
            var element = new CharacterElement(codePoint);
            Assert.AreEqual(codePoint, element.Value);

            var result = element.ToString();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length >= 1); // May be 1 or 2 UTF-16 code units
        }
    }

    #endregion

    #region ElementDelimiter Tests

    [TestMethod]
    public void ElementDelimiter_Properties_SetCorrectly()
    {
        // Arrange
        var element = new CharacterElement(65);

        // Act & Assert
        Assert.AreEqual('\\', element.Delimiter.OpeningSpecifier);
        Assert.AreEqual('\\', element.Delimiter.ClosingSpecifier);
        Assert.AreEqual(ElementStyle.Compact, element.Delimiter.Style);
        Assert.AreEqual(1, element.Delimiter.SpecifierCount);
    }

    [TestMethod]
    public void ElementDelimiter_StaticProperty_MatchesInstance()
    {
        // Arrange
        var element = new CharacterElement(65);

        // Act & Assert
        Assert.AreEqual(CharacterElement.ElementDelimiter.OpeningSpecifier, element.Delimiter.OpeningSpecifier);
        Assert.AreEqual(CharacterElement.ElementDelimiter.ClosingSpecifier, element.Delimiter.ClosingSpecifier);
    }

    #endregion
}
