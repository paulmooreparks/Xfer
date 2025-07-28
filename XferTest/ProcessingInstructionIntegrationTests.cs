using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Configuration;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang;

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Services;
using System.Linq;

namespace XferTest {
    [TestClass]
    public class ProcessingInstructionIntegrationTests {
        [TestMethod]
        public void ParseDocument_WithDeserializePI_InstantiatesCorrectType() {
            string xferDoc = @"<! deserialize { value ""fr-FR"" } !> price *19.99 <! deserialize { value ""en-US"" } !> tax *3.99";
            var doc = XferParser.Parse(xferDoc);
            var piElements = doc.Root.Values.OfType<ProcessingInstructionElement>().ToList();
            Assert.AreEqual(2, piElements.Count);
            Assert.IsTrue(piElements.TrueForAll(pi => pi is DeserializePIElement));
            MetadataElement meta0 = piElements[0] as MetadataElement;
            MetadataElement meta1 = piElements[1] as MetadataElement;
            // Check for PI type key, then inspect nested element for property
            Assert.IsNotNull(meta0);
            Assert.IsTrue(meta0.Values.ContainsKey("deserialize"));
            var pi0Obj = meta0.Values["deserialize"].Value as ObjectElement;
            Assert.IsNotNull(pi0Obj);
            var valueKvp0 = pi0Obj.Values.FirstOrDefault(kvp => kvp.Key == "value");
            Assert.IsNotNull(valueKvp0);
            Assert.AreEqual("fr-FR", ((StringElement)valueKvp0.Value.Value).Value);

            Assert.IsNotNull(meta1);
            Assert.IsTrue(meta1.Values.ContainsKey("deserialize"));
            var pi1Obj = meta1.Values["deserialize"].Value as ObjectElement;
            Assert.IsNotNull(pi1Obj);
            var valueKvp1 = pi1Obj.Values.FirstOrDefault(kvp => kvp.Key == "value");
            Assert.IsNotNull(valueKvp1);
            Assert.AreEqual("en-US", ((StringElement)valueKvp1.Value.Value).Value);
        }

        [TestMethod]
        public void ParseDocument_WithIncludePI_InstantiatesCorrectType() {
            // Print actual keys in .Values for PI element
            string xferDoc = "<! include { path \"other.xfer\" } !>\ndata *42";
            var parser = new Parser();
            var doc = parser.Parse(xferDoc);
            ProcessingInstructionElement? piElement = doc.Root.Values.OfType<ProcessingInstructionElement>().FirstOrDefault();
            Assert.IsNotNull(piElement);
            Assert.IsTrue(piElement is IncludePIElement);
            var metaInc = piElement as MetadataElement;
            Assert.IsNotNull(metaInc);
            Assert.IsTrue(metaInc.Values.ContainsKey("include"));
            var piIncludeObj = metaInc.Values["include"].Value as ObjectElement;
            Assert.IsNotNull(piIncludeObj);
            var pathKvp = piIncludeObj.Values.FirstOrDefault(kvp => kvp.Key == "path");
            Assert.IsNotNull(pathKvp);
            Assert.AreEqual("other.xfer", ((StringElement)pathKvp.Value.Value).Value);
        }

        [TestMethod]
        public void ParseDocument_WithRegularMetadata_DoesNotInstantiatePIType() {
            string xferDoc = "<! author { name \"Alice\" } !>\ndata *123";
            var parser = new Parser();
            var doc = parser.Parse(xferDoc);
            MetadataElement? metaReg = doc.Root.Values.OfType<MetadataElement>().FirstOrDefault(m => !(m is ProcessingInstructionElement));
            Assert.IsNotNull(metaReg);
            var values = metaReg.Values;
            Assert.IsTrue(values.ContainsKey("author"));
            var piAuthorObj = values["author"].Value as ObjectElement;
            Assert.IsNotNull(piAuthorObj);
            var nameKvp = piAuthorObj.Values.FirstOrDefault(kvp => kvp.Key == "name");
            Assert.IsNotNull(nameKvp);
            Assert.AreEqual("Alice", ((StringElement)nameKvp.Value.Value).Value);
        }
    }
}
