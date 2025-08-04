using ParksComputing.Xfer.Lang.Attributes;

namespace Xfer.Service;

public class WeatherForecast {
    public DateOnly Date { get; set; }

    [XferDecimalPrecision(1)]
    public decimal TemperatureC { get; set; }

    [XferDecimalPrecision(1)]
    public decimal TemperatureF => 32 + (TemperatureC / 0.5556m);

    public string? Summary { get; set; }
}

