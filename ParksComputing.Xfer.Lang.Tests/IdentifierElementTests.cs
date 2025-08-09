using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Elements;
using System;

namespace ParksComputing.Xfer.Lang.Tests;

/// <summary>
/// Comprehensive tests for IdentifierElement functionality.
/// Tests identifier validation, delimiter style detection, and serialization behavior.
/// </summary>
[TestClass]
public class IdentifierElementTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_ValidIdentifier_SetsImplicitStyle()
    {
        // Arrange & Act
        var element = new IdentifierElement("validIdentifier");

        // Assert
        Assert.AreEqual("validIdentifier", element.Value);
        Assert.AreEqual("identifier", IdentifierElement.ElementName);
        Assert.AreEqual(':', element.Delimiter.OpeningSpecifier);
        Assert.AreEqual(':', element.Delimiter.ClosingSpecifier);
        Assert.AreEqual(ElementStyle.Implicit, element.Delimiter.Style);
    }

    [TestMethod]
    public void Constructor_WithUnderscores_SetsImplicitStyle()
    {
        // Arrange & Act
        var element = new IdentifierElement("valid_identifier_name");

        // Assert
        Assert.AreEqual("valid_identifier_name", element.Value);
        Assert.AreEqual(ElementStyle.Implicit, element.Delimiter.Style);
    }

    [TestMethod]
    public void Constructor_WithDashes_SetsImplicitStyle()
    {
        // Arrange & Act
        var element = new IdentifierElement("valid-identifier-name");

        // Assert
        Assert.AreEqual("valid-identifier-name", element.Value);
        Assert.AreEqual(ElementStyle.Implicit, element.Delimiter.Style);
    }

    [TestMethod]
    public void Constructor_WithDots_SetsImplicitStyle()
    {
        // Arrange & Act
        var element = new IdentifierElement("valid.identifier.name");

        // Assert
        Assert.AreEqual("valid.identifier.name", element.Value);
        Assert.AreEqual(ElementStyle.Implicit, element.Delimiter.Style);
    }

    [TestMethod]
    public void Constructor_WithNumbers_SetsImplicitStyle()
    {
        // Arrange & Act
        var element = new IdentifierElement("identifier123");

        // Assert
        Assert.AreEqual("identifier123", element.Value);
        Assert.AreEqual(ElementStyle.Implicit, element.Delimiter.Style);
    }

    [TestMethod]
    public void Constructor_WithInvalidCharacters_SetsCompactStyle()
    {
        // Arrange & Act
        var element = new IdentifierElement("invalid identifier");

        // Assert
        Assert.AreEqual("invalid identifier", element.Value);
        Assert.AreEqual(ElementStyle.Compact, element.Delimiter.Style);
    }

    [TestMethod]
    public void Constructor_StartsWithNumber_SetsCompactStyle()
    {
        // Arrange & Act
        var element = new IdentifierElement("123invalid");

        // Assert
        Assert.AreEqual("123invalid", element.Value);
        Assert.AreEqual(ElementStyle.Compact, element.Delimiter.Style);
    }

    [TestMethod]
    public void Constructor_EmptyString_SetsExplicitStyle()
    {
        // Arrange & Act
        var element = new IdentifierElement("");

        // Assert
        Assert.AreEqual("", element.Value);
        Assert.AreEqual(ElementStyle.Explicit, element.Delimiter.Style);
    }

    [TestMethod]
    public void Constructor_EndsWithColon_SetsExplicitStyle()
    {
        // Arrange & Act
        var element = new IdentifierElement("identifier:");

        // Assert
        Assert.AreEqual("identifier:", element.Value);
        Assert.AreEqual(ElementStyle.Explicit, element.Delimiter.Style);
    }

    [TestMethod]
    public void Constructor_CustomSpecifierCount_OverriddenByContent()
    {
        // Arrange & Act - The specifier count gets overridden by CheckAndUpdateDelimiterStyle
        var element = new IdentifierElement("test", specifierCount: 3);

        // Assert - Specifier count is determined by content, not constructor parameter
        Assert.AreEqual(1, element.Delimiter.SpecifierCount); // "test" has no consecutive colons, so count = 0 + 1 = 1
    }

    [TestMethod]
    public void Constructor_ExplicitStyle_OverriddenByContent()
    {
        // Arrange & Act - The style gets overridden by CheckAndUpdateDelimiterStyle
        var element = new IdentifierElement("test", style: ElementStyle.Explicit);

        // Assert - Style is determined by content validation, not constructor parameter
        Assert.AreEqual(ElementStyle.Implicit, element.Delimiter.Style); // "test" is a valid identifier
    }

    #endregion

    #region IsIdentifierLeadingChar Tests

    [TestMethod]
    public void IsIdentifierLeadingChar_Letters_ReturnsTrue()
    {
        // Test uppercase letters
        for (char c = 'A'; c <= 'Z'; c++)
        {
            Assert.IsTrue(IdentifierElement.IsIdentifierLeadingChar(c), $"Letter {c} should be valid leading char");
        }

        // Test lowercase letters
        for (char c = 'a'; c <= 'z'; c++)
        {
            Assert.IsTrue(IdentifierElement.IsIdentifierLeadingChar(c), $"Letter {c} should be valid leading char");
        }
    }

    [TestMethod]
    public void IsIdentifierLeadingChar_Underscore_ReturnsTrue()
    {
        // Arrange & Act & Assert
        Assert.IsTrue(IdentifierElement.IsIdentifierLeadingChar('_'));
    }

    [TestMethod]
    public void IsIdentifierLeadingChar_Numbers_ReturnsFalse()
    {
        // Test digits
        for (char c = '0'; c <= '9'; c++)
        {
            Assert.IsFalse(IdentifierElement.IsIdentifierLeadingChar(c), $"Digit {c} should not be valid leading char");
        }
    }

    [TestMethod]
    public void IsIdentifierLeadingChar_SpecialCharacters_ReturnsFalse()
    {
        var invalidChars = new[] { '-', '.', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '+', '=', ' ' };

        foreach (char c in invalidChars)
        {
            Assert.IsFalse(IdentifierElement.IsIdentifierLeadingChar(c), $"Character {c} should not be valid leading char");
        }
    }

    #endregion

    #region Delimiter Style Detection Tests

    [TestMethod]
    public void DelimiterStyle_ValidIdentifiers_UseImplicitStyle()
    {
        var validIdentifiers = new[]
        {
            "a", "A", "_", "_a", "a1", "A1", "_1",
            "validIdentifier", "valid_identifier", "valid-identifier", "valid.identifier",
            "camelCase", "PascalCase", "snake_case", "kebab-case", "dot.notation",
            "a1b2c3", "test123", "version1_2_3"
        };

        foreach (var identifier in validIdentifiers)
        {
            var element = new IdentifierElement(identifier);
            Assert.AreEqual(ElementStyle.Implicit, element.Delimiter.Style,
                $"Identifier '{identifier}' should use implicit style");
        }
    }

    [TestMethod]
    public void DelimiterStyle_InvalidIdentifiers_UseCompactStyle()
    {
        var invalidIdentifiers = new[]
        {
            "123", "1abc", "test test", "test@domain", "test#tag", "test$var",
            "test%age", "test^power", "test&and", "test*star", "test(paren",
            "test)paren", "test+plus", "test=equal", "test[bracket", "test]bracket",
            "test{brace", "test}brace", "test|pipe", "test\\slash", "test\"quote",
            "test'quote", "test<less", "test>greater", "test?question", "test/slash"
        };

        foreach (var identifier in invalidIdentifiers)
        {
            var element = new IdentifierElement(identifier);
            Assert.AreEqual(ElementStyle.Compact, element.Delimiter.Style,
                $"Identifier '{identifier}' should use compact style");
        }
    }

    [TestMethod]
    public void DelimiterStyle_EndsWithColon_UsesExplicitStyle()
    {
        var endingWithColon = new[]
        {
            ":", "a:", "test:", "valid_identifier:"
        };

        foreach (var identifier in endingWithColon)
        {
            var element = new IdentifierElement(identifier);
            Assert.AreEqual(ElementStyle.Explicit, element.Delimiter.Style,
                $"Identifier '{identifier}' should use explicit style");
        }
    }

    [TestMethod]
    public void DelimiterStyle_EmptyString_UsesExplicitStyle()
    {
        // Arrange & Act
        var element = new IdentifierElement("");

        // Assert
        Assert.AreEqual(ElementStyle.Explicit, element.Delimiter.Style);
    }

    #endregion

    #region Serialization Tests

    [TestMethod]
    public void ToXfer_ImplicitStyle_ReturnsColonDelimited()
    {
        // Arrange
        var element = new IdentifierElement("validIdentifier");

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.AreEqual(":validIdentifier:", result);
    }

    [TestMethod]
    public void ToXfer_CompactStyle_ReturnsProperDelimiting()
    {
        // Arrange
        var element = new IdentifierElement("invalid identifier");

        // Act
        var result = element.ToXfer();

        // Assert
        Console.WriteLine($"Compact style ToXfer result: '{result}'");
        Console.WriteLine($"Actual style: {element.Delimiter.Style}");
        // For compact style, should use Delimiter.Opening and Delimiter.Closing which include angle brackets
        Assert.IsTrue(result.StartsWith("<:"));
        Assert.IsTrue(result.EndsWith(":>"));
        Assert.IsTrue(result.Contains("invalid identifier"));
    }

    [TestMethod]
    public void ToXfer_ExplicitStyle_ReturnsProperDelimiting()
    {
        // Arrange
        var element = new IdentifierElement("identifier:");

        // Act
        var result = element.ToXfer();

        // Assert
        // For explicit style, should use Delimiter.Opening and Delimiter.Closing which include angle brackets
        Assert.IsTrue(result.StartsWith("<:"));
        Assert.IsTrue(result.EndsWith(":>"));
        Assert.IsTrue(result.Contains("identifier:"));
    }

    [TestMethod]
    public void ToXfer_WithFormatting_IgnoresFormattingForImplicit()
    {
        // Arrange
        var element = new IdentifierElement("test");

        // Act
        var result = element.ToXfer(Formatting.Indented, indentation: 4, depth: 2);

        // Assert
        Assert.AreEqual(":test:", result); // Formatting doesn't affect identifier serialization
    }

    #endregion

    #region Consecutive Specifier Tests

    [TestMethod]
    public void ConsecutiveSpecifiers_MultipleColons_AdjustsSpecifierCount()
    {
        var testCases = new[]
        {
            ("test:", 2),      // Ends with 1 colon, needs 2 specifiers (1 + 1)
            ("test::", 3),     // Ends with 2 consecutive colons, needs 3 specifiers (2 + 1)
            ("test:::", 4),    // Ends with 3 consecutive colons, needs 4 specifiers (3 + 1)
            ("te:st", 2),      // 1 colon in middle, needs 2 specifiers (1 + 1)
            (":test", 2),      // Starts with 1 colon, needs 2 specifiers (1 + 1)
        };

        foreach (var (identifier, expectedCount) in testCases)
        {
            var element = new IdentifierElement(identifier);
            Assert.AreEqual(expectedCount, element.Delimiter.SpecifierCount,
                $"Identifier '{identifier}' should have {expectedCount} specifiers");
        }
    }

    #endregion

    #region Unicode and Special Cases

    [TestMethod]
    public void Unicode_Letters_ValidAsLeadingChars()
    {
        // Test various Unicode letter categories
        var unicodeLetters = new[]
        {
            'α', 'β', 'γ',      // Greek
            'а', 'б', 'в',      // Cyrillic
            'أ', 'ب', 'ت',      // Arabic
            'א', 'ב', 'ג',      // Hebrew
        };

        foreach (char c in unicodeLetters)
        {
            Assert.IsTrue(IdentifierElement.IsIdentifierLeadingChar(c),
                $"Unicode letter {c} should be valid leading character");
        }
    }

    [TestMethod]
    public void Unicode_Identifiers_HandleCorrectly()
    {
        var unicodeIdentifiers = new[]
        {
            "αβγ",          // Greek
            "тест",         // Cyrillic
            "テスト",        // Japanese (may be invalid due to non-letter chars)
        };

        foreach (var identifier in unicodeIdentifiers)
        {
            var element = new IdentifierElement(identifier);
            Assert.AreEqual(identifier, element.Value);
            // Style depends on whether characters match the regex pattern
        }
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void EdgeCases_SingleCharacters_HandleCorrectly()
    {
        var singleChars = new[]
        {
            ("a", ElementStyle.Implicit),
            ("A", ElementStyle.Implicit),
            ("_", ElementStyle.Implicit),
            ("1", ElementStyle.Compact),    // Doesn't start with letter/underscore
            ("-", ElementStyle.Implicit),   // Hyphen is allowed as first character in the regex pattern
            (".", ElementStyle.Implicit),   // Dot is allowed as first character in the regex pattern
            (":", ElementStyle.Explicit),   // Empty content or ends with colon
        };

        foreach (var (identifier, expectedStyle) in singleChars)
        {
            var element = new IdentifierElement(identifier);
            Assert.AreEqual(expectedStyle, element.Delimiter.Style,
                $"Single character '{identifier}' should use {expectedStyle} style");
        }
    }

    [TestMethod]
    public void EdgeCases_LongIdentifiers_HandleCorrectly()
    {
        // Very long but valid identifier
        var longIdentifier = new string('a', 1000);
        var element = new IdentifierElement(longIdentifier);

        Assert.AreEqual(longIdentifier, element.Value);
        Assert.AreEqual(ElementStyle.Implicit, element.Delimiter.Style);

        var xfer = element.ToXfer();
        Assert.IsTrue(xfer.StartsWith(":"));
        Assert.IsTrue(xfer.EndsWith(":"));
    }

    [TestMethod]
    public void EdgeCases_WhitespaceOnly_UsesCompactStyle()
    {
        var whitespaceIdentifiers = new[]
        {
            " ",        // Single space
            "  ",       // Multiple spaces
            "\t",       // Tab
            "\n",       // Newline
            " \t\n ",   // Mixed whitespace
        };

        foreach (var identifier in whitespaceIdentifiers)
        {
            var element = new IdentifierElement(identifier);
            Assert.AreEqual(ElementStyle.Compact, element.Delimiter.Style,
                $"Whitespace identifier should use compact style");
        }
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_ReturnsIdentifierValue()
    {
        var identifiers = new[]
        {
            "test",
            "valid_identifier",
            "invalid identifier",
            "123invalid",
            "",
            "identifier:",
        };

        foreach (var identifier in identifiers)
        {
            var element = new IdentifierElement(identifier);
            Assert.AreEqual(identifier, element.ToString());
        }
    }

    #endregion

    #region Element Delimiter Tests

    [TestMethod]
    public void ElementDelimiter_Properties_SetCorrectly()
    {
        // Arrange
        var element = new IdentifierElement("test");

        // Act & Assert
        Assert.AreEqual(':', element.Delimiter.OpeningSpecifier);
        Assert.AreEqual(':', element.Delimiter.ClosingSpecifier);
    }

    [TestMethod]
    public void ElementDelimiter_StaticProperty_MatchesInstance()
    {
        // Arrange
        var element = new IdentifierElement("test");

        // Act & Assert
        Assert.AreEqual(IdentifierElement.ElementDelimiter.OpeningSpecifier, element.Delimiter.OpeningSpecifier);
        Assert.AreEqual(IdentifierElement.ElementDelimiter.ClosingSpecifier, element.Delimiter.ClosingSpecifier);
    }

    #endregion
}
