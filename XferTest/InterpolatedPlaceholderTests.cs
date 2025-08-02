using System;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;

namespace XferTest
{
    [TestClass]
    public class InterpolatedPlaceholderTests
    {
        [TestMethod]
        public void Test_InterpolatedElement_ResolvesEnvironmentVariable()
        {
            var envVarName = "DBPASSWORD";
            var envVarValue = "SuperSecret123!";
            Environment.SetEnvironmentVariable(envVarName, envVarValue);

            var xfer = @"credentials {
                    username ""dbuser""
                    password '<|DBPASSWORD|>'
                }";

            var doc = XferParser.Parse(xfer);
            Assert.IsNotNull(doc, "Document is null.");
            Assert.IsNotNull(doc.Root, "Document root is null.");
            var root = doc.Root!;
            ObjectElement? credentials = null;
            foreach (var element in root.Children)
            {
                if (element is KeyValuePairElement kvp && kvp.Key == "credentials")
                {
                    credentials = kvp.Value as ObjectElement;
                    break;
                }
            }
            Assert.IsNotNull(credentials, "Credentials object not found.");
            InterpolatedElement? passwordElement = null;
            foreach (var kvp in credentials.Dictionary)
            {
                if (kvp.Key == "password")
                {
                    passwordElement = kvp.Value.Value as InterpolatedElement;
                    break;
                }
            }
            Assert.IsNotNull(passwordElement, "Password element not found.");
            var resolvedPassword = passwordElement.Value;
            Assert.AreEqual($"{envVarValue}", resolvedPassword);
        }
    }
}
