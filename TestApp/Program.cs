using System;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.ProcessingInstructions;

namespace TestApp;

class Program
{
    static void Main()
    {
        var pi = new ProcessingInstruction(new StringElement("test"), "test");
        Console.WriteLine($"Style: {pi.Delimiter.Style}");
        Console.WriteLine($"ToXfer(): '{pi.ToXfer()}'");
        Console.WriteLine($"ToString(): '{pi.ToString()}'");

        // Test with explicit style
        pi.Delimiter.Style = ElementStyle.Explicit;
        Console.WriteLine($"Explicit ToXfer(): '{pi.ToXfer()}'");

        // Test with implicit style
        pi.Delimiter.Style = ElementStyle.Implicit;
        Console.WriteLine($"Implicit ToXfer(): '{pi.ToXfer()}'");
    }
}
