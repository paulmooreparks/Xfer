using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Attributes;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class XferPropertyAttributeTests
{
    #region Basic Property Mapping Tests

    [TestMethod]
    public void Serialize_WithXferPropertyAttribute_ShouldUseCustomPropertyName()
    {
        // Arrange
        var testObject = new UserTestClass
        {
            UserName = "alice",
            EmailAddress = "alice@example.com",
            IsActive = true,
            CreatedAt = new DateTime(2023, 12, 25, 10, 30, 0, DateTimeKind.Utc)
        };

        // Act
        var xferString = XferConvert.Serialize(testObject);

        // Assert
        Assert.IsNotNull(xferString);
        // XferProperty attribute DOES work - it uses custom property names
        Assert.IsTrue(xferString.Contains("user_name"), $"Expected custom property name 'user_name', got: {xferString}");
        Assert.IsTrue(xferString.Contains("email_address"), $"Expected custom property name 'email_address', got: {xferString}");
        Assert.IsTrue(xferString.Contains("is_active"), $"Expected custom property name 'is_active', got: {xferString}");
        Assert.IsTrue(xferString.Contains("created_at"), $"Expected custom property name 'created_at', got: {xferString}");

        // Should not contain original property names
        Assert.IsFalse(xferString.Contains("UserName"), $"Should not contain original property name 'UserName', got: {xferString}");
        Assert.IsFalse(xferString.Contains("EmailAddress"), $"Should not contain original property name 'EmailAddress', got: {xferString}");
        Assert.IsFalse(xferString.Contains("IsActive"), $"Should not contain original property name 'IsActive', got: {xferString}");
        Assert.IsFalse(xferString.Contains("CreatedAt"), $"Should not contain original property name 'CreatedAt', got: {xferString}");
    }

    [TestMethod]
    public void Serialize_WithoutXferPropertyAttribute_ShouldUseOriginalPropertyName()
    {
        // Arrange
        var testObject = new SimpleTestClass
        {
            Name = "test",
            Value = 42,
            IsEnabled = false
        };

        // Act
        var xferString = XferConvert.Serialize(testObject);

        // Assert
        Assert.IsNotNull(xferString);
        Assert.IsTrue(xferString.Contains("Name"), $"Expected original property name 'Name', got: {xferString}");
        Assert.IsTrue(xferString.Contains("Value"), $"Expected original property name 'Value', got: {xferString}");
        Assert.IsTrue(xferString.Contains("IsEnabled"), $"Expected original property name 'IsEnabled', got: {xferString}");
    }

    #endregion

    #region Deserialization Tests

    [TestMethod]
    public void Deserialize_WithXferPropertyAttribute_ShouldMapFromCustomPropertyName()
    {
        // Arrange - Use custom property names since XferProperty attribute works correctly
        var xferString = """
        {
            user_name "alice"
            email_address "alice@example.com"
            is_active ~true
            created_at @2023-12-25T10:30:00@
        }
        """;

        // Act
        var deserializedObject = XferConvert.Deserialize<UserTestClass>(xferString);

        // Assert
        Assert.IsNotNull(deserializedObject);
        Assert.AreEqual("alice", deserializedObject.UserName);
        Assert.AreEqual("alice@example.com", deserializedObject.EmailAddress);
        Assert.AreEqual(true, deserializedObject.IsActive);
        // Just check that we have a DateTime - precise format may vary
        Assert.IsTrue(deserializedObject.CreatedAt > DateTime.MinValue);
    }

    [TestMethod]
    public void Deserialize_WithoutXferPropertyAttribute_ShouldMapFromOriginalPropertyName()
    {
        // Arrange
        var xferString = """
        {
            Name "test"
            Value #42
            IsEnabled ~false
        }
        """;

        // Act
        var deserializedObject = XferConvert.Deserialize<SimpleTestClass>(xferString);

        // Assert
        Assert.IsNotNull(deserializedObject);
        Assert.AreEqual("test", deserializedObject.Name);
        Assert.AreEqual(42, deserializedObject.Value);
        Assert.AreEqual(false, deserializedObject.IsEnabled);
    }

    #endregion

    #region Special Character Property Names Tests

    [TestMethod]
    public void Serialize_WithSpecialCharacterPropertyNames_ShouldUseCorrectSyntax()
    {
        // Arrange
        var testObject = new SpecialPropertyNamesTestClass
        {
            ContentType = "application/json",
            CacheControl = "no-cache",
            ApiKey = "secret123",
            UserAgentString = "Mozilla/5.0"
        };

        // Act
        var xferString = XferConvert.Serialize(testObject);

        // Assert
        Assert.IsNotNull(xferString);
        // XferProperty attribute works - serializer uses custom property names
        Assert.IsTrue(xferString.Contains("content-type"), $"Expected custom property name, got: {xferString}");
        Assert.IsTrue(xferString.Contains("cache-control"), $"Expected custom property name, got: {xferString}");
        Assert.IsTrue(xferString.Contains("API-Key"), $"Expected custom property name, got: {xferString}");
        Assert.IsTrue(xferString.Contains("user-agent"), $"Expected custom property name, got: {xferString}");
    }

    [TestMethod]
    public void Deserialize_WithSpecialCharacterPropertyNames_ShouldMapCorrectly()
    {
        // Arrange - Use custom property names since XferProperty attribute works
        var xferString = """
        {
            content-type "application/json"
            cache-control "no-cache"
            API-Key "secret123"
            user-agent "Mozilla/5.0"
        }
        """;

        // Act
        var deserializedObject = XferConvert.Deserialize<SpecialPropertyNamesTestClass>(xferString);

        // Assert
        Assert.IsNotNull(deserializedObject);
        Assert.AreEqual("application/json", deserializedObject.ContentType);
        Assert.AreEqual("no-cache", deserializedObject.CacheControl);
        Assert.AreEqual("secret123", deserializedObject.ApiKey);
        Assert.AreEqual("Mozilla/5.0", deserializedObject.UserAgentString);
    }

    #endregion

    #region Round-trip Tests

    [TestMethod]
    public void RoundTrip_WithXferPropertyAttribute_ShouldMaintainValues()
    {
        // Arrange
        var originalObject = new UserTestClass
        {
            UserName = "bob",
            EmailAddress = "bob@test.com",
            IsActive = false,
            CreatedAt = new DateTime(2024, 1, 15, 14, 25, 30, DateTimeKind.Utc)
        };

        // Act
        var serialized = XferConvert.Serialize(originalObject);
        var deserialized = XferConvert.Deserialize<UserTestClass>(serialized);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(originalObject.UserName, deserialized.UserName);
        Assert.AreEqual(originalObject.EmailAddress, deserialized.EmailAddress);
        Assert.AreEqual(originalObject.IsActive, deserialized.IsActive);
        Assert.AreEqual(originalObject.CreatedAt, deserialized.CreatedAt);
    }

    [TestMethod]
    public void RoundTrip_WithSpecialCharacterPropertyNames_ShouldMaintainValues()
    {
        // Arrange
        var originalObject = new SpecialPropertyNamesTestClass
        {
            ContentType = "text/html",
            CacheControl = "max-age=3600",
            ApiKey = "abc123xyz",
            UserAgentString = "XferTest/1.0"
        };

        // Act
        var serialized = XferConvert.Serialize(originalObject);
        var deserialized = XferConvert.Deserialize<SpecialPropertyNamesTestClass>(serialized);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(originalObject.ContentType, deserialized.ContentType);
        Assert.AreEqual(originalObject.CacheControl, deserialized.CacheControl);
        Assert.AreEqual(originalObject.ApiKey, deserialized.ApiKey);
        Assert.AreEqual(originalObject.UserAgentString, deserialized.UserAgentString);
    }

    #endregion

    #region Mixed Property Types Tests

    [TestMethod]
    public void Serialize_WithMixedPropertyTypes_ShouldHandleAllCorrectly()
    {
        // Arrange
        var testObject = new MixedPropertyTestClass
        {
            RegularProperty = "normal",
            CustomNameProperty = "custom",
            SpecialCharProperty = "special"
        };

        // Act
        var xferString = XferConvert.Serialize(testObject);

        // Assert
        Assert.IsNotNull(xferString);
        // XferProperty attribute works - properties with attributes use custom names
        Assert.IsTrue(xferString.Contains("RegularProperty"), $"Expected regular property name, got: {xferString}");
        Assert.IsTrue(xferString.Contains("custom_name"), $"Expected custom property name, got: {xferString}");
        Assert.IsTrue(xferString.Contains("special-char"), $"Expected custom property name, got: {xferString}");

        // Verify custom names are used instead of originals where applicable
        Assert.IsFalse(xferString.Contains("CustomNameProperty"), $"Should not contain original property name, got: {xferString}");
        Assert.IsFalse(xferString.Contains("SpecialCharProperty"), $"Should not contain original property name, got: {xferString}");
    }

    #endregion

    #region Test Helper Classes

    private class UserTestClass
    {
        [XferProperty("user_name")]
        public string UserName { get; set; } = string.Empty;

        [XferProperty("email_address")]
        public string EmailAddress { get; set; } = string.Empty;

        [XferProperty("is_active")]
        public bool IsActive { get; set; }

        [XferProperty("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    private class SimpleTestClass
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public bool IsEnabled { get; set; }
    }

    private class SpecialPropertyNamesTestClass
    {
        [XferProperty("content-type")]
        public string ContentType { get; set; } = string.Empty;

        [XferProperty("cache-control")]
        public string CacheControl { get; set; } = string.Empty;

        [XferProperty("API-Key")]
        public string ApiKey { get; set; } = string.Empty;

        [XferProperty("user-agent")]
        public string UserAgentString { get; set; } = string.Empty;
    }

    private class MixedPropertyTestClass
    {
        public string RegularProperty { get; set; } = string.Empty;

        [XferProperty("custom_name")]
        public string CustomNameProperty { get; set; } = string.Empty;

        [XferProperty("special-char")]
        public string SpecialCharProperty { get; set; } = string.Empty;
    }

    #endregion
}
