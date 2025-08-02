using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;

namespace XferTest;

[TestClass]
public class DynamicElementPITests {
    [TestMethod]
    public void DynamicElement_ResolvesValue_FromFilePI() {
        // Arrange: create a temp file with a known value
        var tempFile = Path.GetTempFileName();
        var expectedValue = "FileSecretValue123!";
        File.WriteAllText(tempFile, expectedValue);

        var xfer = $@"<! dynamicSource {{ dbpassword file ""{tempFile}"" }} !>
credentials {{
password '<|dbpassword|>'
}}";

        // Act: parse the document
        var doc = XferParser.Parse(xfer);
        var root = doc.Root!;
        ObjectElement? credentials = null;
        foreach (var element in root.Children) {
            if (element is KeyValuePairElement kvp && kvp.Key == "credentials") {
                credentials = kvp.Value as ObjectElement;
                break;
            }
        }
        Assert.IsNotNull(credentials, "Credentials object not found.");
        InterpolatedElement? passwordElement = null;
        foreach (var kvp in credentials.Dictionary) {
            if (kvp.Key == "password") {
                passwordElement = kvp.Value.Value as InterpolatedElement;
                break;
            }
        }
        Assert.IsNotNull(passwordElement, "Password element not found.");
        var resolvedPassword = passwordElement.Value;
        Assert.AreEqual(expectedValue, resolvedPassword);

        // Cleanup
        File.Delete(tempFile);
    }
}
