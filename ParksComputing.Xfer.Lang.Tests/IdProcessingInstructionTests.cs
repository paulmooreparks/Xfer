using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.Tests;

/// <summary>
/// Tests for ID processing instruction functionality to prevent regression bugs.
/// </summary>
[TestClass]
public class IdProcessingInstructionTests
{
    private Parser parser = null!;

    [TestInitialize]
    public void Setup()
    {
        parser = new Parser();
        // No CharDef setup needed for ID tests
    }

    [TestMethod]
    public void IdPI_DocumentLevel_AppliedToRootElement()
    {
        // Arrange
        var xfer = @"
<!id ""root""!>
(
    element1 ""value1""
    element2 ""value2""
)";

        // Act
        var doc = parser.Parse(xfer);
        var rootElement = doc.GetElementById("root");

        // Assert
        Assert.IsNotNull(rootElement, "Root element with ID 'root' should be found");
        Assert.AreEqual("root", rootElement.Id);
        Assert.IsInstanceOfType(rootElement, typeof(TupleElement));
    }

    [TestMethod]
    public void IdPI_TupleLevel_AppliedToNextElement()
    {
        // Arrange
        var xfer = @"(
    <!id ""first""!> element1 ""value1""
    <!id ""second""!> element2 ""value2""
)";

        // Act
        var doc = parser.Parse(xfer);
        var firstElement = doc.GetElementById("first");
        var secondElement = doc.GetElementById("second");

        // Assert
        Assert.IsNotNull(firstElement, "Element with ID 'first' should be found");
        Assert.IsNotNull(secondElement, "Element with ID 'second' should be found");
        Assert.AreEqual("first", firstElement.Id);
        Assert.AreEqual("second", secondElement.Id);
    }

    [TestMethod]
    public void IdPI_ArrayLevel_AppliedToNextElement()
    {
        // Arrange - Test simpler case first
        var xfer = @"[
    <!id ""first""!> 1
]";

        // Act
        var doc = parser.Parse(xfer);
        var firstElement = doc.GetElementById("first");

        // Assert
        Assert.IsNotNull(firstElement, "Element with ID 'first' should be found");
        Assert.AreEqual("first", firstElement.Id);
        Assert.IsInstanceOfType(firstElement, typeof(IntegerElement));
        Assert.AreEqual(1, ((IntegerElement)firstElement).Value);
    }

    [TestMethod]
    public void IdPI_ObjectLevel_AppliedToNextElement()
    {
        // Arrange
        var xfer = @"{
    <!id ""greeting""!> hello ""world""
    <!id ""number""!> answer 42
}";

        // Act
        var doc = parser.Parse(xfer);
        var greetingElement = doc.GetElementById("greeting");
        var numberElement = doc.GetElementById("number");

        // Assert
        Assert.IsNotNull(greetingElement, "Element with ID 'greeting' should be found");
        Assert.IsNotNull(numberElement, "Element with ID 'number' should be found");
        Assert.AreEqual("greeting", greetingElement.Id);
        Assert.AreEqual("number", numberElement.Id);
        Assert.IsInstanceOfType(greetingElement, typeof(KeyValuePairElement));
        Assert.IsInstanceOfType(numberElement, typeof(KeyValuePairElement));
    }

    [TestMethod]
    public void IdPI_KeyValuePair_AppliedToValue()
    {
        // Arrange
        var xfer = @"{
    key <!id ""target""!> ""value""
}";

        // Act
        var doc = parser.Parse(xfer);
        var targetElement = doc.GetElementById("target");

        // Assert
        Assert.IsNotNull(targetElement, "Element with ID 'target' should be found");
        Assert.AreEqual("target", targetElement.Id);
        Assert.IsInstanceOfType(targetElement, typeof(TextElement));
        Assert.AreEqual("value", ((TextElement)targetElement).Value);
    }

    [TestMethod]
    public void IdPI_NestedStructures_AllIdsWork()
    {
        // Arrange - This is similar to the sample.xfer file
        var xfer = @"
<!id ""root""!>
(
    <!id ""kvp""!> foo <!id ""value""!> ""bar""
    <!id ""array""!> [
        <!id ""one""!> 1
        2
        3
    ]
    <!id ""object""!> {
        <!id ""testelem""!> a ""alpha""
        b ""beta""
    }
)";

        // Act
        var doc = parser.Parse(xfer);
        var rootElement = doc.GetElementById("root");
        var kvpElement = doc.GetElementById("kvp");
        var valueElement = doc.GetElementById("value");
        var arrayElement = doc.GetElementById("array");
        var oneElement = doc.GetElementById("one");
        var objectElement = doc.GetElementById("object");
        var testElement = doc.GetElementById("testelem");

        // Assert
        Assert.IsNotNull(rootElement, "Root element should be found");
        Assert.IsNotNull(kvpElement, "KVP element should be found");
        Assert.IsNotNull(valueElement, "Value element should be found");
        Assert.IsNotNull(arrayElement, "Array element should be found");
        Assert.IsNotNull(oneElement, "Element 'one' should be found");
        Assert.IsNotNull(objectElement, "Object element should be found");
        Assert.IsNotNull(testElement, "Test element should be found");

        Assert.AreEqual("root", rootElement.Id);
        Assert.AreEqual("kvp", kvpElement.Id);
        Assert.AreEqual("value", valueElement.Id);
        Assert.AreEqual("array", arrayElement.Id);
        Assert.AreEqual("one", oneElement.Id);
        Assert.AreEqual("object", objectElement.Id);
        Assert.AreEqual("testelem", testElement.Id);

        // Verify element types
        Assert.IsInstanceOfType(rootElement, typeof(TupleElement));
        Assert.IsInstanceOfType(kvpElement, typeof(KeyValuePairElement));
        Assert.IsInstanceOfType(valueElement, typeof(TextElement));
        Assert.IsInstanceOfType(arrayElement, typeof(ArrayElement));
        Assert.IsInstanceOfType(oneElement, typeof(IntegerElement));
        Assert.IsInstanceOfType(objectElement, typeof(ObjectElement));
        Assert.IsInstanceOfType(testElement, typeof(KeyValuePairElement));
    }

    [TestMethod]
    public void IdPI_SpacingVariations_BothWork()
    {
        // Arrange - Test both spacing styles
        var xfer = @"(
    <!id ""nospace""!>element1 ""value1""
    <!id ""withspace""!> element2 ""value2""
)";

        // Act
        var doc = parser.Parse(xfer);
        var noSpaceElement = doc.GetElementById("nospace");
        var withSpaceElement = doc.GetElementById("withspace");

        // Assert
        Assert.IsNotNull(noSpaceElement, "Element with ID 'nospace' should be found");
        Assert.IsNotNull(withSpaceElement, "Element with ID 'withspace' should be found");
        Assert.AreEqual("nospace", noSpaceElement.Id);
        Assert.AreEqual("withspace", withSpaceElement.Id);
    }

    [TestMethod]
    public void IdPI_MultipleInSequence_AllAppliedCorrectly()
    {
        // Arrange - Multiple PIs before single element shouldn't work (last one wins)
        var xfer = @"(
    <!id ""first""!> <!id ""second""!> element ""value""
)";

        // Act
        var doc = parser.Parse(xfer);
        var firstElement = doc.GetElementById("first");
        var secondElement = doc.GetElementById("second");

        // Assert
        // The processing instruction elements themselves should have IDs
        // but the actual target element should have the last PI's ID
        Assert.IsNull(firstElement, "First ID should not be found on target element");
        Assert.IsNotNull(secondElement, "Second ID should be found on target element");
        Assert.AreEqual("second", secondElement.Id);
    }
}
