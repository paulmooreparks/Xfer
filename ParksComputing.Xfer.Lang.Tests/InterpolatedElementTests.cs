using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang;
using System;

namespace ParksComputing.Xfer.Lang.Tests;

/// <summary>
/// Comprehensive tests for InterpolatedElement functionality.
/// Tests interpolated string handling, serialization, and delimiter behavior.
/// </summary>
[TestClass]
public class InterpolatedElementTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_BasicText_SetsValue()
    {
        // Arrange & Act
        var element = new InterpolatedElement("Hello World");

        // Assert
        Assert.AreEqual("Hello World", element.Value);
        Assert.AreEqual("interpolated", InterpolatedElement.ElementName);
        Assert.AreEqual('\'', element.Delimiter.OpeningSpecifier);
        Assert.AreEqual('\'', element.Delimiter.ClosingSpecifier);
        Assert.AreEqual(ElementStyle.Compact, element.Delimiter.Style);
    }

    [TestMethod]
    public void Constructor_EmptyString_SetsValue()
    {
        // Arrange & Act
        var element = new InterpolatedElement("");

        // Assert
        Assert.AreEqual("", element.Value);
        Assert.AreEqual(ElementStyle.Explicit, element.Delimiter.Style);  // Empty string gets Explicit style
    }

    [TestMethod]
    public void Constructor_InterpolatedExpression_SetsValue()
    {
        // Arrange & Act
        var element = new InterpolatedElement("Hello {name}, today is {date}");

        // Assert
        Assert.AreEqual("Hello {name}, today is {date}", element.Value);
    }

    [TestMethod]
    public void Constructor_CustomSpecifierCount_OverriddenByContent()
    {
        // Arrange & Act - The specifier count gets overridden by CheckAndUpdateDelimiterStyle
        var element = new InterpolatedElement("test", specifierCount: 3);

        // Assert - Specifier count is determined by content, not constructor parameter
        Assert.AreEqual(1, element.Delimiter.SpecifierCount); // "test" has no single quotes, so count = 0 + 1 = 1
    }

    [TestMethod]
    public void Constructor_ExplicitStyle_SetsCorrectly()
    {
        // Arrange & Act
        var element = new InterpolatedElement("test", style: ElementStyle.Explicit);

        // Assert
        Assert.AreEqual(ElementStyle.Explicit, element.Delimiter.Style);
    }

    [TestMethod]
    public void Constructor_ImplicitStyle_OverriddenByContent()
    {
        // Arrange & Act - The style gets overridden by CheckAndUpdateDelimiterStyle
        var element = new InterpolatedElement("test", style: ElementStyle.Implicit);

        // Assert - Style is determined by content validation, not constructor parameter
        Assert.AreEqual(ElementStyle.Compact, element.Delimiter.Style); // "test" is non-empty and doesn't end with quote
    }

    #endregion

    #region Inheritance Tests

    [TestMethod]
    public void Inheritance_IsTextElement()
    {
        // Arrange & Act
        var element = new InterpolatedElement("test");

        // Assert
        Assert.IsInstanceOfType(element, typeof(TextElement));
    }

    [TestMethod]
    public void Inheritance_HasTextElementProperties()
    {
        // Arrange
        var text = "Hello World";
        var element = new InterpolatedElement(text);

        // Act & Assert
        Assert.AreEqual(text, element.Value);
        Assert.AreEqual(text, element.ToString());
    }

    #endregion

    #region Serialization Tests

    [TestMethod]
    public void ToXfer_BasicString_ReturnsCorrectFormat()
    {
        // Arrange
        var element = new InterpolatedElement("Hello World");

        // Act
        var result = element.ToXfer();

        // Assert - Should be Compact style for normal text
        Assert.AreEqual(ElementStyle.Compact, element.Delimiter.Style);
        Assert.AreEqual("'Hello World'", result);
    }

    [TestMethod]
    public void ToXfer_ExplicitStyle_ReturnsCorrectFormat()
    {
        // Arrange - create with explicit style directly
        var element = new InterpolatedElement("test", 1, ElementStyle.Explicit);

        // Act
        var result = element.ToXfer();

        // Assert - Explicit style uses Opening/Closing with angle brackets
        Assert.AreEqual(ElementStyle.Explicit, element.Delimiter.Style);
        Assert.AreEqual("<'test'>", result);
    }

    [TestMethod]
    public void ToXfer_EmptyString_ReturnsCorrectFormat()
    {
        // Arrange - empty string gets Explicit style due to CheckAndUpdateDelimiterStyle
        var element = new InterpolatedElement("");

        // Act
        var result = element.ToXfer();

        // Assert - Explicit style uses Opening/Closing with angle brackets
        Assert.AreEqual(ElementStyle.Explicit, element.Delimiter.Style);
        Assert.AreEqual("<''>", result);
    }

    [TestMethod]
    public void ToXfer_WithSingleQuotes_HandlesCorrectly()
    {
        // Arrange - content with single quotes should get increased specifier count
        var element = new InterpolatedElement("It's a test");

        // Act
        var result = element.ToXfer();

        // Assert - Should be Compact style with increased specifier count
        Assert.AreEqual(ElementStyle.Compact, element.Delimiter.Style);
        Assert.IsTrue(element.Delimiter.SpecifierCount >= 2); // Increased due to single quote
        var expectedOpening = new string('\'', element.Delimiter.SpecifierCount);
        var expectedClosing = expectedOpening;
        Assert.AreEqual($"{expectedOpening}It's a test{expectedClosing}", result);
    }

    [TestMethod]
    public void ToXfer_WithInterpolation_HandlesCorrectly()
    {
        // Arrange
        var element = new InterpolatedElement("Hello {name}!");

        // Act
        var result = element.ToXfer();

        // Assert - Should be Compact style for normal text
        Assert.AreEqual(ElementStyle.Compact, element.Delimiter.Style);
        Assert.AreEqual("'Hello {name}!'", result);
    }

    [TestMethod]
    public void ToXfer_WithFormatting_IgnoresFormatting()
    {
        // Arrange
        var element = new InterpolatedElement("test");

        // Act
        var result = element.ToXfer(Formatting.Indented, indentation: 4, depth: 2);

        // Assert - TextElement doesn't really use formatting for simple values
        Assert.AreEqual("'test'", result);
    }

    #endregion

    #region Interpolation Expression Tests

    [TestMethod]
    public void InterpolationExpressions_SimpleVariable_HandlesCorrectly()
    {
        var expressions = new[]
        {
            "Hello {name}",
            "The answer is {answer}",
            "Today is {date}",
            "{greeting} World!",
        };

        foreach (var expression in expressions)
        {
            var element = new InterpolatedElement(expression);
            Assert.AreEqual(expression, element.Value);

            var xfer = element.ToXfer();
            Assert.IsTrue(xfer.Contains(expression));
        }
    }

    [TestMethod]
    public void InterpolationExpressions_MultipleVariables_HandlesCorrectly()
    {
        var expressions = new[]
        {
            "Hello {firstName} {lastName}",
            "{greeting} {name}, today is {date}",
            "The {adjective} {noun} is {color}",
            "{a} + {b} = {result}",
        };

        foreach (var expression in expressions)
        {
            var element = new InterpolatedElement(expression);
            Assert.AreEqual(expression, element.Value);
        }
    }

    [TestMethod]
    public void InterpolationExpressions_NestedBraces_HandlesCorrectly()
    {
        var expressions = new[]
        {
            "Object: {{key: value}}",
            "Array: [{item1}, {item2}]",
            "Function: {func({param})}",
            "Conditional: {condition ? {trueValue} : {falseValue}}",
        };

        foreach (var expression in expressions)
        {
            var element = new InterpolatedElement(expression);
            Assert.AreEqual(expression, element.Value);
        }
    }

    [TestMethod]
    public void InterpolationExpressions_NoInterpolation_HandlesCorrectly()
    {
        var expressions = new[]
        {
            "Plain text",
            "No variables here",
            "Just a simple string",
            "Text with { but no closing",
            "Text with } but no opening",
        };

        foreach (var expression in expressions)
        {
            var element = new InterpolatedElement(expression);
            Assert.AreEqual(expression, element.Value);
        }
    }

    #endregion

    #region Special Character Tests

    [TestMethod]
    public void SpecialCharacters_Quotes_HandlesCorrectly()
    {
        var testCases = new[]
        {
            "Single quote: '",
            "Double quote: \"",
            "Mixed quotes: 'hello \"world\"'",
            "Escaped quote: \\'",
        };

        foreach (var text in testCases)
        {
            var element = new InterpolatedElement(text);
            Assert.AreEqual(text, element.Value);
        }
    }

    [TestMethod]
    public void SpecialCharacters_Newlines_HandlesCorrectly()
    {
        var testCases = new[]
        {
            "Line 1\nLine 2",
            "Line 1\r\nLine 2",
            "Multiple\n\nLines",
            "Tab\there",
        };

        foreach (var text in testCases)
        {
            var element = new InterpolatedElement(text);
            Assert.AreEqual(text, element.Value);
        }
    }

    [TestMethod]
    public void SpecialCharacters_Unicode_HandlesCorrectly()
    {
        var testCases = new[]
        {
            "Hello 世界",           // Chinese
            "Привет мир",          // Russian
            "مرحبا بالعالم",        // Arabic
            "Hello {name} 👋",     // Emoji
            "Math: α + β = γ",     // Greek letters
        };

        foreach (var text in testCases)
        {
            var element = new InterpolatedElement(text);
            Assert.AreEqual(text, element.Value);
        }
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void EdgeCases_VeryLongText_HandlesCorrectly()
    {
        // Very long interpolated string
        var longText = new string('a', 10000) + " {variable} " + new string('b', 10000);
        var element = new InterpolatedElement(longText);

        Assert.AreEqual(longText, element.Value);

        var xfer = element.ToXfer();
        Assert.IsTrue(xfer.Contains(longText));
    }

    [TestMethod]
    public void EdgeCases_OnlyBraces_HandlesCorrectly()
    {
        var testCases = new[]
        {
            "{",
            "}",
            "{}",
            "{{}}",
            "{{}",
            "{}}",
            "{{{{",
            "}}}}",
        };

        foreach (var text in testCases)
        {
            var element = new InterpolatedElement(text);
            Assert.AreEqual(text, element.Value);
        }
    }

    [TestMethod]
    public void EdgeCases_OnlyQuotes_HandlesCorrectly()
    {
        var testCases = new[]
        {
            "'",
            "''",
            "'''",
            "''''",
        };

        foreach (var text in testCases)
        {
            var element = new InterpolatedElement(text);
            Assert.AreEqual(text, element.Value);
        }
    }

    [TestMethod]
    public void EdgeCases_WhitespaceOnly_HandlesCorrectly()
    {
        var testCases = new[]
        {
            " ",
            "  ",
            "\t",
            "\n",
            "\r\n",
            "   \t\n   ",
        };

        foreach (var text in testCases)
        {
            var element = new InterpolatedElement(text);
            Assert.AreEqual(text, element.Value);
        }
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_ReturnsValue()
    {
        var testTexts = new[]
        {
            "Hello World",
            "Hello {name}",
            "",
            "Complex {a} + {b} = {result}",
            "Unicode: 世界",
            "Special: 'quotes' and \"more quotes\"",
        };

        foreach (var text in testTexts)
        {
            var element = new InterpolatedElement(text);
            Assert.AreEqual(text, element.ToString());
        }
    }

    #endregion

    #region Element Delimiter Tests

    [TestMethod]
    public void ElementDelimiter_Properties_SetCorrectly()
    {
        // Arrange
        var element = new InterpolatedElement("test");

        // Act & Assert
        Assert.AreEqual('\'', element.Delimiter.OpeningSpecifier);
        Assert.AreEqual('\'', element.Delimiter.ClosingSpecifier);
        Assert.AreEqual(ElementStyle.Compact, element.Delimiter.Style);
        Assert.AreEqual(1, element.Delimiter.SpecifierCount);
    }

    [TestMethod]
    public void ElementDelimiter_StaticProperty_MatchesInstance()
    {
        // Arrange
        var element = new InterpolatedElement("test");

        // Act & Assert
        Assert.AreEqual(InterpolatedElement.ElementDelimiter.OpeningSpecifier, element.Delimiter.OpeningSpecifier);
        Assert.AreEqual(InterpolatedElement.ElementDelimiter.ClosingSpecifier, element.Delimiter.ClosingSpecifier);
    }

    #endregion

    #region Comparison with StringElement

    [TestMethod]
    public void ComparisonWithStringElement_DifferentDelimiters()
    {
        // Arrange
        var text = "Hello World";
        var interpolatedElement = new InterpolatedElement(text);
        var stringElement = new StringElement(text);

        // Act & Assert
        Assert.AreEqual(text, interpolatedElement.Value);
        Assert.AreEqual(text, stringElement.Value);
        Assert.AreNotEqual(interpolatedElement.Delimiter.OpeningSpecifier, stringElement.Delimiter.OpeningSpecifier);
        Assert.AreEqual('\'', interpolatedElement.Delimiter.OpeningSpecifier);
        Assert.AreEqual('"', stringElement.Delimiter.OpeningSpecifier);
    }

    [TestMethod]
    public void ComparisonWithStringElement_SameValue()
    {
        // Arrange
        var text = "Hello {name}";
        var interpolatedElement = new InterpolatedElement(text);
        var stringElement = new StringElement(text);

        // Act & Assert
        Assert.AreEqual(text, interpolatedElement.Value);
        Assert.AreEqual(text, stringElement.Value);
        Assert.AreEqual(interpolatedElement.ToString(), stringElement.ToString());
    }

    #endregion
}
