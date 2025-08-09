using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang;

class TestProcessingInstruction : ProcessingInstruction
{
    public TestProcessingInstruction() : base(new StringElement("test"), "test") { }

    public override void ElementHandler(Element element) { }
    public override void ProcessingInstructionHandler() { }
}

class Program
{
    static void Main()
    {
        // Test exactly what the failing test does
        var pi = new TestProcessingInstruction();
        pi.AddChild(new StringElement("hello"));

        var result = pi.ToString();
        Console.WriteLine($"ToString() result: '{result}'");
        Console.WriteLine($"Contains 'test\"test\"': {result.Contains("test\"test\"")}");
        Console.WriteLine($"Contains '\"hello\"': {result.Contains("\"hello\"")}");

        // Also test ToXfer
        var xferResult = pi.ToXfer();
        Console.WriteLine($"\nToXfer() result: '{xferResult}'");
    }
}
