using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Elements;
using System;

namespace ParksComputing.Xfer.Lang.Tests
{
    [TestClass]
    public class DebugToXferTests
    {
        [TestMethod]
        public void DebugIdentifierFormats()
        {
            var invalidIdent = new IdentifierElement("invalid identifier");
            var toXferResult = invalidIdent.ToXfer();

            Console.WriteLine($"Style: {invalidIdent.Delimiter.Style}");
            Console.WriteLine($"ToXfer: '{toXferResult}'");
            Console.WriteLine($"Starts with colon: {toXferResult.StartsWith(":")}");
            Console.WriteLine($"Ends with colon: {toXferResult.EndsWith(":")}");

            // This test will always pass to see the output
            Assert.IsTrue(true);
        }
    }
}
