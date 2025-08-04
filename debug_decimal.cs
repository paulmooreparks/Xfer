using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Attributes;
using System;

class DecimalPrecisionTestClass
{
    [XferDecimalPrecision(2)]
    public decimal Price { get; set; }

    [XferDecimalPrecision(4, RemoveTrailingZeros = false)]
    public decimal Interest { get; set; }

    [XferDecimalPrecision(1)]
    public double Temperature { get; set; }

    [XferDecimalPrecision(0)]
    public decimal Quantity { get; set; }

    // Without attribute - uses default precision
    public decimal Cost { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        // Test the serialization/deserialization
        var originalValue = 123.456789m;
        var testObject = new DecimalPrecisionTestClass { Price = originalValue };

        Console.WriteLine($"Original Price: {testObject.Price}");

        var xferString = XferConvert.Serialize(testObject);
        Console.WriteLine($"Serialized: {xferString}");

        // Parse to see the document structure
        var document = XferParser.Parse(xferString);
        Console.WriteLine($"Document valid: {document.IsValid}");
        Console.WriteLine($"Root has values: {document.Root.Values.Any()}");
        Console.WriteLine($"Root values count: {document.Root.Values.Count()}");
        Console.WriteLine($"Root children count: {document.Root.Children.Count()}");

        foreach (var value in document.Root.Values)
        {
            Console.WriteLine($"Value type: {value.GetType().Name}");
            Console.WriteLine($"Value content: {value}");
        }

        try
        {
            var deserializedObject = XferConvert.Deserialize<DecimalPrecisionTestClass>(xferString);
            if (deserializedObject == null)
            {
                Console.WriteLine("Deserialization returned null!");
            }
            else
            {
                Console.WriteLine($"Deserialized Price: {deserializedObject.Price}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Deserialization error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
