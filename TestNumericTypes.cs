using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Configuration;

namespace XferConsoleTest;

class Program
{
    static void Main()
    {
        // Test different numeric types with default settings
        var data = new
        {
            intValue = 42,
            longValue = 42L,
            doubleValue = 42.0,
            decimalValue = 42.0m
        };

        string serialized = XferConvert.Serialize(data, Formatting.None);
        Console.WriteLine($"Default serialization: {serialized}");

        // Test with explicit preference
        var explicitSettings = new XferSerializerSettings
        {
            StylePreference = ElementStylePreference.Explicit
        };

        string explicitSerialized = XferConvert.Serialize(data, explicitSettings, Formatting.None);
        Console.WriteLine($"Explicit serialization: {explicitSerialized}");
    }
}
