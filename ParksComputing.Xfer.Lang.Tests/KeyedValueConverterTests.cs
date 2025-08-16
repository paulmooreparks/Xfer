using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Configuration;
using ParksComputing.Xfer.Lang.Attributes;

namespace ParksComputing.Xfer.Lang.Tests {
    [TestClass]
    public class KeyedValueConverterTests {
        private static T? RoundTrip<T>(string xfer) => XferConvert.Deserialize<T>(xfer);

        public class ScriptHolder {
            [XferProperty("script")] // map to lowercase key in Xfer document
            public XferKeyedValue? Script { get; set; }
        }

        [TestMethod]
        public void Deserialize_SimplePayload_IntoKeyedValue() {
            var dto = RoundTrip<ScriptHolder>("{ script \"scriptBody()\" }");
            Assert.IsNotNull(dto);
            Assert.IsNotNull(dto!.Script);
            Assert.AreEqual(0, dto!.Script!.Keys.Count);
            Assert.AreEqual("scriptBody()", dto.Script!.PayloadAsString);
        }

        [TestMethod]
        public void Deserialize_OneKeyword_IntoKeyedValue() {
            var dto = RoundTrip<ScriptHolder>("{ script javascript \"scriptBody()\" }");
            Assert.IsNotNull(dto);
            Assert.IsNotNull(dto!.Script);
            CollectionAssert.AreEqual(new[] { "javascript" }, new System.Collections.Generic.List<string>(dto!.Script!.Keys));
            Assert.AreEqual("scriptBody()", dto.Script!.PayloadAsString);
        }

        [TestMethod]
        public void Deserialize_MultipleKeywords_IntoKeyedValue() {
            var dto = RoundTrip<ScriptHolder>("{ script javascript preparse ecmascript2025 \"scriptBody()\" }");
            Assert.IsNotNull(dto);
            Assert.IsNotNull(dto!.Script);
            CollectionAssert.AreEqual(new[] { "javascript", "preparse", "ecmascript2025" }, new System.Collections.Generic.List<string>(dto!.Script!.Keys));
            Assert.AreEqual("scriptBody()", dto.Script!.PayloadAsString);
        }

        [TestMethod]
        public void Serialize_KeyedValue_WrapsKeys() {
            var kv = new XferKeyedValue(["javascript"], new Elements.StringElement("body"));
            var obj = new ScriptHolder { Script = kv };
            var xfer = XferConvert.Serialize(obj, Formatting.None);
            // No space before quoted string in compact mode: name"value"
            StringAssert.Contains(xfer, "script javascript\"body\"");
        }
    }
}
