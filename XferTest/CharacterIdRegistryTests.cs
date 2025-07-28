using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Services;

namespace XferTest;

[TestClass]
public class CharacterIdRegistryTests {
    [TestMethod]
    public void BuiltinIds_AreResolvedCorrectly() {
        Assert.AreEqual(0x0D, CharacterIdRegistry.Resolve("cr"));
        Assert.AreEqual(0x0A, CharacterIdRegistry.Resolve("lf"));
        Assert.AreEqual(0x3C, CharacterIdRegistry.Resolve("lt"));
        Assert.AreEqual(0x3E, CharacterIdRegistry.Resolve("gt"));
        Assert.AreEqual(0x00, CharacterIdRegistry.Resolve("nul"));
        Assert.AreEqual(0x09, CharacterIdRegistry.Resolve("tab"));
    }

    [TestMethod]
    public void CustomIds_OverrideBuiltin() {
        var custom = new Dictionary<string, int> {
            { "cr", 0x99 },
            { "custom1", 0x42 }
        };
        CharacterIdRegistry.SetCustomIds(custom);
        Assert.AreEqual(0x99, CharacterIdRegistry.Resolve("cr"));
        Assert.AreEqual(0x42, CharacterIdRegistry.Resolve("custom1"));
    }

    [TestMethod]
    public void CustomIds_CaseInsensitive() {
        var custom = new Dictionary<string, int> {
            { "MyChar", 0x77 }
        };
        CharacterIdRegistry.SetCustomIds(custom);
        Assert.AreEqual(0x77, CharacterIdRegistry.Resolve("mychar"));
        Assert.AreEqual(0x77, CharacterIdRegistry.Resolve("MYCHAR"));
    }

    [TestMethod]
    public void UnknownId_ReturnsNull() {
        CharacterIdRegistry.SetCustomIds(new Dictionary<string, int>());
        Assert.IsNull(CharacterIdRegistry.Resolve("notfound"));
    }
}
