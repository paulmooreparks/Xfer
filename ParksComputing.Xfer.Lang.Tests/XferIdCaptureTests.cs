using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Attributes;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class XferIdCaptureTests {
    private static string Wrap(string inner) => "{\n" + inner + "\n}";

    private static T? RoundTrip<T>(string xfer) => XferConvert.Deserialize<T>(xfer);

    private class IdCaptureModel {
        [XferProperty("name")]
        public string? Name { get; set; }
        [XferCaptureId(nameof(Name))]
        public string? NameId { get; set; }
    }

    private class CombinedCaptureModel {
        [XferProperty("name")]
        public string? Name { get; set; }
        [XferCaptureId(nameof(Name))]
        public string? NameId { get; set; }
        [XferCaptureTag(nameof(Name))]
        public string? NameTag { get; set; }
    }

    [TestMethod]
    public void Capture_Id_Into_String() {
        var xfer = Wrap("""
            <!id "foo"!>
            name "bar"
        """);

        var dto = RoundTrip<IdCaptureModel>(xfer);
        Assert.IsNotNull(dto);
        Assert.AreEqual("bar", dto!.Name);
        Assert.AreEqual("foo", dto.NameId);
    }

    [TestMethod]
    public void Capture_NoId_Yields_Null() {
        var xfer = Wrap("name \"bar\"");

        var dto = RoundTrip<IdCaptureModel>(xfer);
        Assert.IsNotNull(dto);
        Assert.AreEqual("bar", dto!.Name);
        Assert.IsNull(dto.NameId);
    }

    [TestMethod]
    public void Capture_Id_And_Tag_Together() {
        var xfer = Wrap("""
            <!tag "t"!>
            <!id "z42"!>
            name "bar"
        """);

        var dto = RoundTrip<CombinedCaptureModel>(xfer);
        Assert.IsNotNull(dto);
        Assert.AreEqual("bar", dto!.Name);
        Assert.AreEqual("z42", dto.NameId);
        Assert.AreEqual("t", dto.NameTag);
    }

    private class SkipSerializeModel {
        [XferProperty("name")]
        public string? Name { get; set; }
        [XferCaptureId(nameof(Name))]
        public string? NameId { get; set; }
        [XferCaptureTag(nameof(Name))]
        public string[]? NameTags { get; set; }
    }

    [TestMethod]
    public void Serialize_Should_Skip_Capture_Target_Properties() {
        var model = new SkipSerializeModel {
            Name = "Widget",
            NameId = "id-123",
            NameTags = new [] { "A", "B" }
        };

    var xfer = XferConvert.Serialize(model, Formatting.None);
    // Should contain only 'name', not NameId/NameTags because they are capture targets
    StringAssert.Contains(xfer, "name");
    StringAssert.Contains(xfer, "\"Widget\"");
        Assert.IsFalse(xfer.Contains("NameId"), "NameId should not be serialized");
        Assert.IsFalse(xfer.Contains("NameTags"), "NameTags should not be serialized");
    }

    private class ConflictModel {
        [XferProperty("name")]
        public string? Name { get; set; }
        [XferCaptureId(nameof(Name))]
        [XferCaptureTag(nameof(Name))]
        public string? Bad { get; set; }
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Serialize_Should_Throw_On_Both_Capture_Attributes() {
        var model = new ConflictModel { Name = "a", Bad = "b" };
        _ = XferConvert.Serialize(model, Formatting.None);
    }
}
