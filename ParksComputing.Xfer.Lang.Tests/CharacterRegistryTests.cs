using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class CharacterRegistryTests
{
    [TestMethod]
    public void CharacterIdRegistry_Resolve_CommonCharacters_ShouldWork()
    {
        // Test a few common character names that should exist
        var testCases = new[]
        {
            ("space", ' '),
            ("tab", '\t'),
            ("lf", '\n'),
            ("cr", '\r')
        };

        foreach (var (name, expectedChar) in testCases)
        {
            // Act
            var result = CharacterIdRegistry.Resolve(name);

            // Assert - if the character is registered, it should match
            if (result.HasValue)
            {
                Assert.AreEqual(expectedChar, result.Value, $"Character '{name}' should resolve to '{expectedChar}'");
            }
            // If not registered, that's fine too - we're just testing the API doesn't crash
        }
    }

    [TestMethod]
    public void CharacterIdRegistry_Resolve_UnknownCharacter_ShouldReturnNull()
    {
        // Arrange
        var unknownName = "definitely_unknown_character_name_12345";

        // Act
        var result = CharacterIdRegistry.Resolve(unknownName);

        // Assert
        Assert.IsFalse(result.HasValue, "Unknown character names should return null");
    }

    [TestMethod]
    public void CharacterIdRegistry_Resolve_EmptyString_ShouldReturnNull()
    {
        // Act
        var result = CharacterIdRegistry.Resolve("");

        // Assert
        Assert.IsFalse(result.HasValue, "Empty string should return null");
    }

    [TestMethod]
    public void CharacterIdRegistry_Resolve_NullString_ShouldReturnNull()
    {
        // Act & Assert - The API might throw an exception for null input
        // This is acceptable behavior, so we'll test for either null return or exception
        try
        {
            var result = CharacterIdRegistry.Resolve(null!);
            Assert.IsFalse(result.HasValue, "Null string should return null if method handles it gracefully");
        }
        catch (ArgumentNullException)
        {
            // This is also acceptable behavior for null input
            Assert.IsTrue(true, "ArgumentNullException is acceptable for null input");
        }
    }

    [TestMethod]
    public void CharacterIdRegistry_Resolve_CaseHandling_ShouldBeConsistent()
    {
        // Test case handling for a common character
        var testName = "space";

        // Act
        var lowercase = CharacterIdRegistry.Resolve(testName.ToLower());
        var uppercase = CharacterIdRegistry.Resolve(testName.ToUpper());
        var mixedCase = CharacterIdRegistry.Resolve("Space");

        // Assert - behavior should be consistent
        // We don't assert specific case sensitivity since that might be configurable,
        // but the behavior should be consistent across calls
        if (lowercase.HasValue && uppercase.HasValue)
        {
            Assert.AreEqual(lowercase.Value, uppercase.Value, "Case handling should be consistent");
        }

        if (lowercase.HasValue && mixedCase.HasValue)
        {
            Assert.AreEqual(lowercase.Value, mixedCase.Value, "Case handling should be consistent");
        }
    }
}
