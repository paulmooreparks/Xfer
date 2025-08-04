using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class IdUniquenessTests
{
    [TestMethod]
    public void ParseDocument_WithUniqueIds_ShouldSucceed()
    {
        // Arrange
        var parser = new Parser();
        var xferInput = """
            {
                <! id "element1" !> name "John"
                <! id "element2" !> age 30
                <! id "element3" !> city "New York"
            }
            """;

        // Act & Assert - Should not throw
        var document = parser.Parse(xferInput);

        // Verify the document was parsed successfully
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ParseDocument_WithDuplicateIds_ShouldThrowException()
    {
        // Arrange
        var parser = new Parser();
        var xferInput = """
            {
                <! id "duplicate" !> name "John"
                <! id "duplicate" !> age 30
            }
            """;

        // Act - Should throw InvalidOperationException
        parser.Parse(xferInput);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ParseDocument_WithDuplicateIdsInNestedElements_ShouldThrowException()
    {
        // Arrange
        var parser = new Parser();
        var xferInput = """
            {
                <! id "outer" !> person {
                    <! id "outer" !> name "John"
                    age 30
                }
            }
            """;

        // Act - Should throw InvalidOperationException
        parser.Parse(xferInput);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ParseDocument_WithDuplicateIdsInArray_ShouldThrowException()
    {
        // Arrange
        var parser = new Parser();
        var xferInput = """
            [
                <! id "item" !> "first"
                <! id "item" !> "second"
            ]
            """;

        // Act - Should throw InvalidOperationException
        parser.Parse(xferInput);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ParseDocument_WithEmptyId_ShouldThrowException()
    {
        // Arrange
        var parser = new Parser();
        var xferInput = """
            {
                <! id "" !> name "John"
            }
            """;

        // Act - Should throw InvalidOperationException
        parser.Parse(xferInput);
    }

    [TestMethod]
    public void ParseDocument_MultipleDocuments_ShouldResetIdTracking()
    {
        // Arrange
        var parser = new Parser();
        var xferInput1 = """
            {
                <! id "test" !> name "John"
            }
            """;
        var xferInput2 = """
            {
                <! id "test" !> name "Jane"
            }
            """;

        // Act & Assert - Should not throw, as each document should reset ID tracking
        var document1 = parser.Parse(xferInput1);
        var document2 = parser.Parse(xferInput2);

        Assert.IsNotNull(document1);
        Assert.IsNotNull(document2);
    }

    [TestMethod]
    public void Parse_WithValidUniqueIds_ShouldNotThrow()
    {
        // Arrange
        var parser = new Parser();
        string xferDocument = @"{
            <! id ""valid-id-1"" !> element1 ""value1""
            <! id ""valid-id-2"" !> element2 ""value2""
        }";

        // Act & Assert - Should not throw
        Assert.IsNotNull(parser.Parse(xferDocument));
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Parse_WithDuplicateIds_ShouldThrowException()
    {
        // Arrange
        var parser = new Parser();
        string xferDocument = @"{
            <! id ""duplicate-id"" !> element1 ""value1""
            <! id ""duplicate-id"" !> element2 ""value2""
        }";

        // Act - Should throw InvalidOperationException
        parser.Parse(xferDocument);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Parse_WithNullId_ShouldThrowException()
    {
        // Arrange
        var parser = new Parser();
        // This should fail during parsing because processing instruction needs a key-value pair
        string xferDocument = @"{<! id !> element ""value""}";

        // Act - Should throw InvalidOperationException
        parser.Parse(xferDocument);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Parse_WithEmptyId_ShouldThrowException()
    {
        // Arrange
        var parser = new Parser();
        string xferDocument = @"{<! id """" !> element ""value""}";

        // Act - Should throw InvalidOperationException
        parser.Parse(xferDocument);
    }
}
