using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang;

namespace DebugProcessingInstruction;

public class SimpleTestClass
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public bool IsActive { get; set; }
}

class Program
{
    static void Main()
    {
        // Test XferConvert.Serialize like the failing test
        var original = new SimpleTestClass
        {
            Name = "Test User",
            Age = 25,
            IsActive = true
        };

        var xferContent = XferConvert.Serialize(original, Formatting.Pretty);
        Console.WriteLine("XferConvert.Serialize result:");
        Console.WriteLine($"'{xferContent}'");

        // Also test manual object creation to compare
        var manualObj = new ObjectElement();
        manualObj["Name"] = new StringElement("Test User");
        manualObj["Age"] = new IntegerElement(25, elementStyle: ElementStyle.Implicit);
        manualObj["IsActive"] = new BooleanElement(true);

        var manualResult = manualObj.ToXfer(Formatting.Pretty);
        Console.WriteLine("\nManual ObjectElement result:");
        Console.WriteLine($"'{manualResult}'");
    }
}
