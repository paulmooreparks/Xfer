using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Elements;

namespace XferTest {
    [TestClass]
    public class ProcessingInstructionTests {
        [TestMethod]
        public void DeserializePIElement_StoresAndRetrievesValues() {
            var pi = new DeserializePIElement();
            var kvp = new KeyValuePairElement(new KeywordElement("value"), new StringElement("fr-FR"));
            pi.Add(kvp);
            Assert.AreEqual("fr-FR", ((StringElement)pi["value"].Value).Value);
            Assert.AreEqual(DeserializePIElement.DeserializeKeyword, pi.PIType);
        }

        [TestMethod]
        public void IncludePIElement_StoresAndRetrievesValues() {
            var pi = new IncludePIElement();
            var kvp = new KeyValuePairElement(new KeywordElement("path"), new StringElement("other.xfer"));
            pi.Add(kvp);
            Assert.AreEqual("other.xfer", ((StringElement)pi["path"].Value).Value);
            Assert.AreEqual(IncludePIElement.IncludeKeyword, pi.PIType);
        }

        [TestMethod]
        public void ProcessingInstructionElement_EqualityAndType() {
            var pi1 = new DeserializePIElement();
            var pi2 = new IncludePIElement();
            Assert.AreNotEqual(pi1.PIType, pi2.PIType);
            Assert.IsTrue(pi1 is ProcessingInstructionElement);
            Assert.IsTrue(pi2 is ProcessingInstructionElement);
        }
    }
}
