using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Elements;
using System;

namespace ParksComputing.Xfer.Lang.Tests;

/// <summary>
/// Comprehensive tests for DateElement functionality.
/// Tests DateOnly storage, serialization, and DateTimeHandling behavior.
/// </summary>
[TestClass]
public class DateElementTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_DefaultDateTimeHandling_SetsCorrectly()
    {
        // Arrange & Act
        var element = new DateElement();

        // Assert
        Assert.AreEqual(DateTimeHandling.RoundTrip, element.DateTimeHandling);
        Assert.AreEqual(DateElement.ElementName, DateTimeElement.ElementName);
        Assert.AreEqual('@', element.Delimiter.OpeningSpecifier);
        Assert.AreEqual('@', element.Delimiter.ClosingSpecifier);
        Assert.AreEqual(ElementStyle.Compact, element.Delimiter.Style); // Fixed: Should default to Compact, not Explicit
    }

    [TestMethod]
    public void Constructor_WithDateOnly_SetsValue()
    {
        // Arrange
        var date = new DateOnly(2023, 12, 25);

        // Act
        var element = new DateElement(date);

        // Assert
        Assert.AreEqual(date, element.Value);
        Assert.AreEqual(DateTimeHandling.RoundTrip, element.DateTimeHandling);
    }

    [TestMethod]
    public void Constructor_WithDateTimeHandling_SetsCorrectly()
    {
        // Arrange
        var date = new DateOnly(2023, 12, 25);

        // Act
        var element = new DateElement(date, DateTimeHandling.Local);

        // Assert
        Assert.AreEqual(date, element.Value);
        Assert.AreEqual(DateTimeHandling.Local, element.DateTimeHandling);
    }

    [TestMethod]
    public void Constructor_WithStringValue_ParsesCorrectly()
    {
        // Arrange & Act
        var element = new DateElement("2023-12-25");

        // Assert
        Assert.AreEqual(new DateOnly(2023, 12, 25), element.Value);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Constructor_WithInvalidString_ThrowsException()
    {
        // Arrange & Act
        new DateElement("invalid-date");
    }

    [TestMethod]
    public void Constructor_WithCustomSpecifierCount_SetsCorrectly()
    {
        // Arrange & Act
        var element = new DateElement(new DateOnly(2023, 12, 25), specifierCount: 2);

        // Assert - Should respect the specifierCount parameter
        Assert.AreEqual(2, element.Delimiter.SpecifierCount);
    }

    [TestMethod]
    public void Constructor_WithElementStyle_SetsCorrectly()
    {
        // Arrange & Act
        var element = new DateElement(new DateOnly(2023, 12, 25), elementStyle: ElementStyle.Explicit);

        // Assert
        Assert.AreEqual(ElementStyle.Explicit, element.Delimiter.Style);
    }

    #endregion

    #region Serialization Tests

    [TestMethod]
    public void ToXfer_RoundTripHandling_ReturnsISO8601Format()
    {
        // Arrange
        var date = new DateOnly(2023, 12, 25);
        var element = new DateElement(date, DateTimeHandling.RoundTrip);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.IsTrue(result.Contains("2023-12-25"));
        Assert.IsTrue(result.StartsWith("<@"));
    }

    [TestMethod]
    public void ToXfer_LeapYear_HandlesCorrectly()
    {
        // Arrange
        var date = new DateOnly(2024, 2, 29); // Leap year
        var element = new DateElement(date);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.IsTrue(result.Contains("2024-02-29"));
    }

    [TestMethod]
    public void ToXfer_EdgeDates_HandleCorrectly()
    {
        // Test various edge cases
        var testCases = new[]
        {
            new DateOnly(1, 1, 1),          // Minimum date
            new DateOnly(9999, 12, 31),     // Maximum date
            new DateOnly(2000, 1, 1),       // Y2K
            new DateOnly(1900, 2, 28),      // Non-leap year Feb 28
            new DateOnly(2000, 2, 29),      // Leap year Feb 29
        };

        foreach (var date in testCases)
        {
            var element = new DateElement(date);
            var result = element.ToXfer();

            Assert.IsTrue(result.StartsWith("<@"), $"Date {date} should start with <@");
            Assert.IsTrue(result.Contains(date.ToString("yyyy-MM-dd")), $"Date {date} should contain ISO format");
        }
    }

    #endregion

    #region DateTimeHandling Tests

    [TestMethod]
    public void DateTimeHandling_RoundTrip_PreservesOriginalFormat()
    {
        // Arrange
        var date = new DateOnly(2023, 12, 25);
        var element = new DateElement(date, DateTimeHandling.RoundTrip);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.IsTrue(result.Contains("2023-12-25"));
    }

    [TestMethod]
    public void DateTimeHandling_Local_UsesLocalFormat()
    {
        // Arrange
        var date = new DateOnly(2023, 12, 25);
        var element = new DateElement(date, DateTimeHandling.Local);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.StartsWith("<@"));
    }

    [TestMethod]
    public void DateTimeHandling_UTC_UsesUTCFormat()
    {
        // Arrange
        var date = new DateOnly(2023, 12, 25);
        var element = new DateElement(date, DateTimeHandling.Utc);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.StartsWith("<@"));
    }

    [TestMethod]
    public void DateTimeHandling_Unspecified_UsesUnspecifiedFormat()
    {
        // Arrange
        var date = new DateOnly(2023, 12, 25);
        var element = new DateElement(date, DateTimeHandling.Unspecified);

        // Act
        var result = element.ToXfer();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.StartsWith("<@"));
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_ReturnsDateString()
    {
        // Arrange
        var date = new DateOnly(2023, 12, 25);
        var element = new DateElement(date);

        // Act
        var result = element.ToString();

        // Assert
        Assert.AreEqual("2023-12-25", result);
    }

    [TestMethod]
    public void ToString_DifferentDates_ReturnCorrectStrings()
    {
        var testCases = new[]
        {
            new DateOnly(2023, 1, 1),
            new DateOnly(2023, 6, 15),
            new DateOnly(2023, 12, 31),
        };

        foreach (var date in testCases)
        {
            var element = new DateElement(date);
            var result = element.ToString();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("2023"));
        }
    }

    #endregion

    #region Special Date Cases

    [TestMethod]
    public void SpecialDates_Holidays_HandleCorrectly()
    {
        var holidays = new[]
        {
            new DateOnly(2023, 1, 1),   // New Year's Day
            new DateOnly(2023, 7, 4),   // Independence Day
            new DateOnly(2023, 12, 25), // Christmas
            new DateOnly(2023, 11, 23), // Thanksgiving (example)
        };

        foreach (var holiday in holidays)
        {
            var element = new DateElement(holiday);

            Assert.AreEqual(holiday, element.Value);

            var xfer = element.ToXfer();
            Assert.IsTrue(xfer.StartsWith("<@"));

            var str = element.ToString();
            Assert.IsNotNull(str);
        }
    }

    [TestMethod]
    public void SpecialDates_LeapYears_HandleCorrectly()
    {
        var leapYearDates = new[]
        {
            new DateOnly(2000, 2, 29), // Divisible by 400
            new DateOnly(2004, 2, 29), // Divisible by 4
            new DateOnly(2020, 2, 29), // Recent leap year
            new DateOnly(2024, 2, 29), // Future leap year
        };

        foreach (var date in leapYearDates)
        {
            var element = new DateElement(date);

            Assert.AreEqual(date, element.Value);
            Assert.AreEqual(29, element.Value.Day);
            Assert.AreEqual(2, element.Value.Month);
        }
    }

    #endregion

    #region Element Delimiter Tests

    [TestMethod]
    public void ElementDelimiter_Properties_SetCorrectly()
    {
        // Arrange
        var element = new DateElement();

        // Act & Assert
        Assert.AreEqual('@', element.Delimiter.OpeningSpecifier);
        Assert.AreEqual('@', element.Delimiter.ClosingSpecifier);
        Assert.AreEqual(ElementStyle.Compact, element.Delimiter.Style); // Fixed: Should default to Compact, not Explicit
        Assert.AreEqual(1, element.Delimiter.SpecifierCount);
    }

    [TestMethod]
    public void ElementDelimiter_StaticProperty_MatchesInstance()
    {
        // Arrange
        var element = new DateElement();

        // Act & Assert
        Assert.AreEqual(DateElement.ElementDelimiter.OpeningSpecifier, element.Delimiter.OpeningSpecifier);
        Assert.AreEqual(DateElement.ElementDelimiter.ClosingSpecifier, element.Delimiter.ClosingSpecifier);
        Assert.AreEqual(DateTimeElement.ElementDelimiter.OpeningSpecifier, element.Delimiter.OpeningSpecifier);
    }

    #endregion

    #region Parsing Edge Cases

    [TestMethod]
    public void Parsing_ValidISO8601Formats_ParseCorrectly()
    {
        var validFormats = new[]
        {
            "2023-12-25",
            "2023-01-01",
            "2023-06-15",
            "1999-12-31",
        };

        foreach (var format in validFormats)
        {
            var element = new DateElement(format);
            var expectedDate = DateOnly.Parse(format);

            Assert.AreEqual(expectedDate, element.Value);
        }
    }

    [TestMethod]
    public void Parsing_InvalidFormats_ThrowExceptions()
    {
        var invalidFormats = new[]
        {
            "",
            "not-a-date",
            "2023-13-01", // Invalid month
            "2023-12-32", // Invalid day
        };

        foreach (var format in invalidFormats)
        {
            Assert.ThrowsException<InvalidOperationException>(() => new DateElement(format),
                $"Should throw exception for invalid format: {format}");
        }
    }

    #endregion
}
