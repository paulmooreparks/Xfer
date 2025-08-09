using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.ProcessingInstructions;

namespace DebugProcessingInstruction;

// Create a test processing instruction class similar to the one in tests
public class TestProcessingInstruction : ProcessingInstruction
{
    public TestProcessingInstruction() : base("test", "test") { }
}

class Program
{
    static void Main()
    {
        // Replicate the test: ToXfer_WithStringChild_ReturnsCorrectFormat
        var pi = new TestProcessingInstruction();
        pi.AddChild(new StringElement("hello"));

        var result = pi.ToXfer();

        Console.WriteLine($"Actual result: '{result}'");
        Console.WriteLine($"Contains 'test\"test\"': {result.Contains("test\"test\"")}");
        Console.WriteLine($"Contains '\"hello\"': {result.Contains("\"hello\"")}");
        Console.WriteLine($"Starts with '<!': {result.StartsWith("<!")}");
        Console.WriteLine($"Ends with '!>': {result.EndsWith("!>")}");

        // Show what each part looks like
        Console.WriteLine("\nBreaking down the result:");
        Console.WriteLine($"Full result: {result}");
        for (int i = 0; i < result.Length; i++)
        {
            if (result[i] == ' ')
                Console.Write("[SPACE]");
            else
                Console.Write(result[i]);
        }
        Console.WriteLine();
    }
}
