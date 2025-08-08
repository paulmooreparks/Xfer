using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.ProcessingInstructions;
using ParksComputing.Xfer.Lang.Services;
using System;
using System.Linq;

namespace ParksComputing.Xfer.Lang.Tests;

/// <summary>
/// Tests for the "defined" processing instruction functionality.
/// </summary>
[TestClass]
public class DefinedProcessingInstructionTests
{
    private Parser parser = null!;

    [TestInitialize]
    public void Setup()
    {
        parser = new Parser();
        // Clear any existing dynamic source configurations
        DynamicSourceRegistry.Clear();
    }

    [TestMethod]
    public void DefinedPI_TextElement_AlwaysReturnsTrue()
    {
        // Arrange - Text elements are always considered defined
        var xfer = @"
<! defined ""TEST_STRING"" !>
{
    message ""Hello World""
}";

        // Act
        var doc = parser.Parse(xfer);
        var definedPI = doc.ProcessingInstructions.OfType<DefinedProcessingInstruction>().FirstOrDefault();

        // Assert
        Assert.IsNotNull(definedPI, "DefinedProcessingInstruction should be found");
        Assert.IsTrue(definedPI.IsDefined, "Text elements should always be considered defined");
    }

    [TestMethod]
    public void DefinedPI_TextElement_AlwaysReturnsTrue_Case2()
    {
        // Arrange - Text elements are always considered defined
        var xfer = @"
<! defined ""NONEXISTENT_TEST_VAR"" !>
{
    message ""Hello World""
}";

        // Act
        var doc = parser.Parse(xfer);
        var definedPI = doc.ProcessingInstructions.OfType<DefinedProcessingInstruction>().FirstOrDefault();

        // Assert
        Assert.IsNotNull(definedPI, "DefinedProcessingInstruction should be found");
        Assert.IsTrue(definedPI.IsDefined, "Text elements should always be considered defined");
    }

    [TestMethod]
    public void DefinedPI_TextElement_AlwaysReturnsTrue_Case3()
    {
        // Arrange - Text elements are always considered defined
        var xfer = @"
<! defined ""EXISTING_ENV_TEST"" !>
{
    message ""Hello World""
}";

        // Act
        var doc = parser.Parse(xfer);
        var definedPI = doc.ProcessingInstructions.OfType<DefinedProcessingInstruction>().FirstOrDefault();

        // Assert
        Assert.IsNotNull(definedPI, "DefinedProcessingInstruction should be found");
        Assert.IsTrue(definedPI.IsDefined, "Text elements should always be considered defined");
    }

    [TestMethod]
    public void DefinedPI_EmptyStringElement_ReturnsTrue()
    {
        // Arrange - Text elements are always considered defined, even empty ones
        var xfer = @"
<! defined <""""> !>
{
    message ""Hello World""
}";

        // Act
        var doc = parser.Parse(xfer);
        var definedPI = doc.ProcessingInstructions.OfType<DefinedProcessingInstruction>().FirstOrDefault();

        // Assert
        Assert.IsNotNull(definedPI, "DefinedProcessingInstruction should be found");
        Assert.IsTrue(definedPI.IsDefined, "Text elements should always be considered defined, even empty ones");
    }

    [TestMethod]
    public void DefinedPI_TextElement_AlwaysReturnsTrue_Case4()
    {
        // Arrange - Text elements are always considered defined
        var xfer = @"
<! defined ""unregistered_key"" !>
{
    message ""Hello World""
}";

        // Act
        var doc = parser.Parse(xfer);
        var definedPI = doc.ProcessingInstructions.OfType<DefinedProcessingInstruction>().FirstOrDefault();

        // Assert
        Assert.IsNotNull(definedPI, "DefinedProcessingInstruction should be found");
        Assert.IsTrue(definedPI.IsDefined, "Text elements should always be considered defined");
    }

    [TestMethod]
    public void DefinedPI_TextElement_AlwaysReturnsTrue_Case5()
    {
        // Arrange - Text elements are always considered defined
        var xfer = @"
<! defined ""PATH"" !>
{
    message ""Hello World""
}";

        // Act
        var doc = parser.Parse(xfer);
        var definedPI = doc.ProcessingInstructions.OfType<DefinedProcessingInstruction>().FirstOrDefault();

        // Assert
        Assert.IsNotNull(definedPI, "DefinedProcessingInstruction should be found");
        Assert.IsTrue(definedPI.IsDefined, "Text elements should always be considered defined");
    }

    [TestMethod]
    public void DefinedPI_CharacterElement_AlwaysReturnsTrue()
    {
        // Arrange
        var xfer = @"
<! defined \$42 !>
{
    message ""Hello World""
}";

        // Act
        var doc = parser.Parse(xfer);
        var definedPI = doc.ProcessingInstructions.OfType<DefinedProcessingInstruction>().FirstOrDefault();

        // Assert
        Assert.IsNotNull(definedPI, "DefinedProcessingInstruction should be found");
        Assert.IsTrue(definedPI.IsDefined, "Character elements should always be considered defined");
        Assert.IsInstanceOfType(definedPI.SourceElement, typeof(CharacterElement));
    }

    [TestMethod]
    public void DefinedPI_DynamicElement_EnvironmentVar_ReturnsCorrectResult()
    {
        // Arrange - Set a test environment variable
        Environment.SetEnvironmentVariable("DYNAMIC_TEST_VAR", "test_value");

        var xfer = @"
<! defined <|DYNAMIC_TEST_VAR|> !>
{
    message ""Hello World""
}";

        // Act
        var doc = parser.Parse(xfer);
        var definedPI = doc.ProcessingInstructions.OfType<DefinedProcessingInstruction>().FirstOrDefault();

        // Assert
        Assert.IsNotNull(definedPI, "DefinedProcessingInstruction should be found");
        Assert.IsTrue(definedPI.IsDefined, "Dynamic element with existing env var should be defined");
        Assert.IsInstanceOfType(definedPI.SourceElement, typeof(DynamicElement));

        // Cleanup
        Environment.SetEnvironmentVariable("DYNAMIC_TEST_VAR", null);
    }

    [TestMethod]
    public void DefinedPI_DynamicElement_UndefinedVar_ReturnsFalse()
    {
        // Arrange - Ensure variable doesn't exist
        Environment.SetEnvironmentVariable("UNDEFINED_DYNAMIC_VAR", null);

        var xfer = @"
<! defined <|UNDEFINED_DYNAMIC_VAR|> !>
{
    message ""Hello World""
}";

        // Act
        var doc = parser.Parse(xfer);
        var definedPI = doc.ProcessingInstructions.OfType<DefinedProcessingInstruction>().FirstOrDefault();

        // Assert
        Assert.IsNotNull(definedPI, "DefinedProcessingInstruction should be found");
        Assert.IsFalse(definedPI.IsDefined, "Dynamic element with undefined env var should not be defined");
        Assert.IsInstanceOfType(definedPI.SourceElement, typeof(DynamicElement));
    }

    [TestMethod]
    public void DefinedPI_DynamicElement_WithDynamicSource_ReturnsTrue()
    {
        // Arrange
        var xfer = @"
<! dynamicSource {
    testkey const ""test_value""
} !>
<! defined <|testkey|> !>
{
    message ""Hello World""
}";

        // Act
        var doc = parser.Parse(xfer);
        var definedPI = doc.ProcessingInstructions.OfType<DefinedProcessingInstruction>().FirstOrDefault();

        // Assert
        Assert.IsNotNull(definedPI, "DefinedProcessingInstruction should be found");
        Assert.IsTrue(definedPI.IsDefined, "Dynamic element with configured source should be defined");
        Assert.IsInstanceOfType(definedPI.SourceElement, typeof(DynamicElement));
    }

    [TestMethod]
    public void DefinedPI_IntegerElement_AlwaysReturnsTrue()
    {
        // Arrange
        var xfer = @"
<! defined 123 !>
{
    message ""Hello World""
}";

        // Act
        var doc = parser.Parse(xfer);
        var definedPI = doc.ProcessingInstructions.OfType<DefinedProcessingInstruction>().FirstOrDefault();

        // Assert
        Assert.IsNotNull(definedPI, "DefinedProcessingInstruction should be found");
        Assert.IsTrue(definedPI.IsDefined, "Integer elements should always be considered defined");
        Assert.IsInstanceOfType(definedPI.SourceElement, typeof(IntegerElement));
    }

    [TestMethod]
    public void DefinedPI_ToString_ShowsElementAndResult()
    {
        // Arrange
        var xfer = @"
<! defined ""TEST_TOSTRING_VAR"" !>
{
    message ""Hello World""
}";

        // Act
        var doc = parser.Parse(xfer);
        var definedPI = doc.ProcessingInstructions.OfType<DefinedProcessingInstruction>().FirstOrDefault();

        // Assert
        Assert.IsNotNull(definedPI, "DefinedProcessingInstruction should be found");
        var toStringResult = definedPI.ToString();
        Assert.IsTrue(toStringResult.Contains("TEST_TOSTRING_VAR"), "ToString should contain the element value");
        Assert.IsTrue(toStringResult.Contains("True"), "ToString should contain the result");
    }
}
