using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.Tests
{
    [TestClass]
    public class ExplicitElementFormatTests
    {
        [TestMethod]
        public void LongElement_ExplicitStyle_ShouldUseAngleBrackets()
        {
            // Arrange
            var element = new LongElement(42, 1, ElementStyle.Explicit);

            // Act
            var result = element.ToXfer();

            // Assert
            Assert.IsTrue(result.StartsWith("<&"), $"Expected explicit format to start with '<&', but got: '{result}'");
            Assert.IsTrue(result.EndsWith("&>"), $"Expected explicit format to end with '&>', but got: '{result}'");
            Assert.AreEqual("<&42&>", result, "Explicit format should be '<&42&>'");
        }

        [TestMethod]
        public void DateElement_ExplicitStyle_ShouldUseAngleBrackets()
        {
            // Arrange
            var element = new DateElement(new DateOnly(2023, 1, 15), DateTimeHandling.RoundTrip, 1, ElementStyle.Explicit);

            // Act
            var result = element.ToXfer();

            // Assert
            Assert.IsTrue(result.StartsWith("<@"), $"Expected explicit format to start with '<@', but got: '{result}'");
            Assert.IsTrue(result.EndsWith("@>"), $"Expected explicit format to end with '@>', but got: '{result}'");
        }

        [TestMethod]
        public void ObjectElement_ExplicitStyle_UsesAngleBrackets_CorrectReference()
        {
            // Arrange - ObjectElement should correctly use angle brackets (reference implementation)
            var element = new ObjectElement();
            element.Delimiter.Style = ElementStyle.Explicit;

            // Act
            var result = element.ToXfer();

            // Assert
            Assert.IsTrue(result.StartsWith("<{"), $"Expected explicit format to start with '<{{', but got: '{result}'");
            Assert.IsTrue(result.EndsWith("}>"), $"Expected explicit format to end with '}}>', but got: '{result}'");
        }
    }
}
