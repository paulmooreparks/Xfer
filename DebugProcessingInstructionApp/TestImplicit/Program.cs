using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang;

class TestImplicit
{
    static void Main()
    {
        // Test with implicit style elements that should need spaces
        var implicitInt = new IntegerElement(42);
        implicitInt.Delimiter = new ElementDelimiter(1, style: ElementStyle.Implicit);

        var implicitString = new StringElement("test");
        implicitString.Delimiter = new ElementDelimiter(1, style: ElementStyle.Implicit);

        var kvpImplicitInt = new KeyValuePairElement(new KeywordElement("count"), implicitInt);
        var kvpImplicitString = new KeyValuePairElement(new KeywordElement("word"), implicitString);

        Console.WriteLine($"Implicit int: '{kvpImplicitInt.ToXfer()}'");
        Console.WriteLine($"Implicit string: '{kvpImplicitString.ToXfer()}'");
        Console.WriteLine($"Int delimiter style: {implicitInt.Delimiter.Style}");
        Console.WriteLine($"String delimiter style: {implicitString.Delimiter.Style}");
        Console.WriteLine($"Int ToXfer: '{implicitInt.ToXfer()}'");
        Console.WriteLine($"String ToXfer: '{implicitString.ToXfer()}'");
    }
}
