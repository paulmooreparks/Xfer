using System;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Configuration;
using ParksComputing.Xfer.Lang.Converters;
using ParksComputing.Xfer.Lang.Elements;

// Simple test to see what custom converters actually produce
public class Coordinate
{
    public double Latitude { get; }
    public double Longitude { get; }

    public Coordinate(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}

public class CoordinateConverter : XferConverter<Coordinate>
{
    public override Element WriteXfer(Coordinate value, XferSerializerSettings settings)
    {
        return new StringElement($"{value.Latitude},{value.Longitude}");
    }

    public override Coordinate ReadXfer(Element element, XferSerializerSettings settings)
    {
        if (element is StringElement stringElement)
        {
            var parts = stringElement.Value.Split(',');
            if (parts.Length == 2 &&
                double.TryParse(parts[0], out double lat) &&
                double.TryParse(parts[1], out double lng))
            {
                return new Coordinate(lat, lng);
            }
        }
        throw new InvalidOperationException("Cannot convert element to Coordinate.");
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Testing Custom Converter System...");

        // Test 1: Without custom converter
        var coordinate = new Coordinate(40.7128, -74.0060);
        var withoutConverter = XferConvert.Serialize(coordinate);
        Console.WriteLine($"Without converter: {withoutConverter}");

        // Test 2: With custom converter
        var settings = new XferSerializerSettings();
        settings.Converters.Add(new CoordinateConverter());
        var withConverter = XferConvert.Serialize(coordinate, settings);
        Console.WriteLine($"With converter: {withConverter}");

        // Test 3: Check if they're different
        Console.WriteLine($"Are they different? {withoutConverter != withConverter}");

        // Test 4: Deserialize test
        var deserialized = XferConvert.Deserialize<Coordinate>(withConverter, settings);
        Console.WriteLine($"Deserialized: {deserialized.Latitude}, {deserialized.Longitude}");
    }
}
