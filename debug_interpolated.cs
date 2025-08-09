using ParksComputing.Xfer.Lang.Elements;
using System;

class DebugInterpolated
{
    static void Main()
    {
        var element = new InterpolatedElement("");
        Console.WriteLine($"Style: {element.Delimiter.Style}");
        Console.WriteLine($"SpecifierCount: {element.Delimiter.SpecifierCount}");
        Console.WriteLine($"Opening: '{element.Delimiter.Opening}'");
        Console.WriteLine($"Closing: '{element.Delimiter.Closing}'");
        Console.WriteLine($"MinOpening: '{element.Delimiter.MinOpening}'");
        Console.WriteLine($"MinClosing: '{element.Delimiter.MinClosing}'");
        Console.WriteLine($"ToXfer result: '{element.ToXfer()}'");
    }
}
