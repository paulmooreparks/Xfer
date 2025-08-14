using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Attributes;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class XferTagCaptureTests {
    private static string Wrap(string inner) => $"{{\n{inner}\n}}";

    private static T? RoundTrip<T>(string xfer) => XferConvert.Deserialize<T>(xfer);

    private class StringCaptureModel {
        [XferProperty("id")]
        public string? Id { get; set; }
        [XferCaptureTag(nameof(Id))]
        public string? IdTag { get; set; }
    }

    private class ListCaptureModel {
        [XferProperty("name")]
        public string? Name { get; set; }
        [XferCaptureTag(nameof(Name))]
        public List<string>? NameTags { get; set; }
    }

    private class ArrayCaptureModel {
        [XferProperty("role")]
        public string? Role { get; set; }
        [XferCaptureTag(nameof(Role))]
        public string[]? RoleTags { get; set; }
    }

    [TestMethod]
    public void Capture_FirstTag_Into_String() {
        var xfer = Wrap("""
            <!tag "first"!>
            <!tag "second"!>
            id "X"
        """);

        var dto = RoundTrip<StringCaptureModel>(xfer);
        Assert.IsNotNull(dto);
        Assert.AreEqual("X", dto!.Id);
        Assert.AreEqual("first", dto.IdTag);
    }

    [TestMethod]
    public void Capture_AllTags_Into_List() {
        var xfer = Wrap("""
            <!tag "A"!>
            <!tag "B"!>
            name "Alice"
        """);

        var dto = RoundTrip<ListCaptureModel>(xfer);
        Assert.IsNotNull(dto);
        Assert.AreEqual("Alice", dto!.Name);
        CollectionAssert.AreEqual(new[] { "A", "B" }, dto.NameTags ?? new List<string>());
    }

    [TestMethod]
    public void Capture_AllTags_Into_Array() {
        var xfer = Wrap("""
            <!tag "admin"!>
            <!tag "ops"!>
            role "maintainer"
        """);

        var dto = RoundTrip<ArrayCaptureModel>(xfer);
        Assert.IsNotNull(dto);
        Assert.AreEqual("maintainer", dto!.Role);
        CollectionAssert.AreEqual(new[] { "admin", "ops" }, dto.RoleTags ?? Array.Empty<string>());
    }

    [TestMethod]
    public void Capture_NoTags_Yields_EmptyOrNulls() {
        var xfer1 = Wrap("id \"Y\"");
        var dto1 = RoundTrip<StringCaptureModel>(xfer1);
        Assert.IsNotNull(dto1);
        Assert.AreEqual("Y", dto1!.Id);
        Assert.IsNull(dto1.IdTag); // string target -> null when no tags

        var xfer2 = Wrap("name \"Bob\"");
        var dto2 = RoundTrip<ListCaptureModel>(xfer2);
        Assert.IsNotNull(dto2);
        Assert.AreEqual("Bob", dto2!.Name);
        Assert.IsNotNull(dto2.NameTags); // list target -> empty list
        Assert.AreEqual(0, dto2.NameTags!.Count);

        var xfer3 = Wrap("role \"reader\"");
        var dto3 = RoundTrip<ArrayCaptureModel>(xfer3);
        Assert.IsNotNull(dto3);
        Assert.AreEqual("reader", dto3!.Role);
        Assert.IsNotNull(dto3.RoleTags); // array target -> empty array
        Assert.AreEqual(0, dto3.RoleTags!.Length);
    }
}
