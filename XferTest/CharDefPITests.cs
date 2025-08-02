using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;

namespace XferTest {
    [TestClass]
    public class CharDefPITests {
        [TestMethod]
        public void CharDefPI_AddsCustomCharacterIds() {
            // Example xfer document with charDef PI
            string xferDoc = @"<!charDef { nl \$0A foo \$42 bar \$99 } !> '<\foo\>'";
            var doc = XferParser.Parse(xferDoc);
            // After parsing, custom char IDs should be registered
            Assert.AreEqual(0x0A, CharacterIdRegistry.Resolve("nl"));
            Assert.AreEqual(0x42, CharacterIdRegistry.Resolve("foo"));
            Assert.AreEqual(0x99, CharacterIdRegistry.Resolve("bar"));
        }

        [TestMethod]
        public void CharDefPI_OverridesBuiltinIds() {
            string xferDoc = @"<!charDef { cr \$99 lf \$42 } !> 'text'";
            var doc = XferParser.Parse(xferDoc);
            Assert.AreEqual(0x99, CharacterIdRegistry.Resolve("cr"));
            Assert.AreEqual(0x42, CharacterIdRegistry.Resolve("lf"));
        }

        [TestMethod]
        public void CharDefPI_CharacterElement_ResolvesCustomId() {
            // charDef PI defines 'foo' as 0x42, then uses </foo/>
            string xferDoc = @"<!charDef { foo \$42 } !> ( <\foo\> )";
            var doc = XferParser.Parse(xferDoc);
            // Recursively search for CharacterElement in the document tree
            Assert.IsTrue(doc.Root.Values.Where(v => v is CharacterElement ce && ce.Value == 0x42).Any(), "Character element with value 0x42 not found.");
        }

        [TestMethod]
        public void CharDefPI_CaseInsensitiveKeys() {
            string xferDoc = @"<!charDef { MyChar \$77 } !> '<\mychar\>'";
            var doc = XferParser.Parse(xferDoc);
            Assert.AreEqual(0x77, CharacterIdRegistry.Resolve("mychar"));
            Assert.AreEqual(0x77, CharacterIdRegistry.Resolve("MYCHAR"));
        }

        [TestMethod]
        public void CharDefPI_UnknownId_ReturnsNull() {
            string xferDoc = @"<!charDef { foo \$42 } !> '<\foo\>'";
            var doc = XferParser.Parse(xferDoc);
            Assert.IsNull(CharacterIdRegistry.Resolve("notfound"));
        }
    }
}
