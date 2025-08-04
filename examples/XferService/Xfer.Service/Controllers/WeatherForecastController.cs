using Microsoft.AspNetCore.Mvc;

namespace Xfer.Service.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase {
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger) {
        _logger = logger;
    }

    [HttpGet(Name = "WeatherForecast")]
    public ActionResult<IEnumerable<WeatherForecast>> Get() {
        return Enumerable.Range(1, 5).Select(index => {
            var temperatureC = (Random.Shared.Next(-200, 555)) / 10m;
            var summary = GetSummaryForTemperature(temperatureC);
            
            return new WeatherForecast {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = temperatureC,
                Summary = summary
            };
        })
        .ToArray();
    }

    /// <summary>
    /// Determines the appropriate weather summary based on temperature in Celsius.
    /// </summary>
    /// <param name="temperatureC">Temperature in Celsius</param>
    /// <returns>Weather summary that matches the temperature range</returns>
    private static string GetSummaryForTemperature(decimal temperatureC) {
        return temperatureC switch {
            < -10m => "Freezing",     // Below -10°C
            < 0m => "Bracing",        // -10°C to 0°C
            < 5m => "Chilly",         // 0°C to 5°C
            < 15m => "Cool",          // 5°C to 15°C
            < 20m => "Mild",          // 15°C to 20°C
            < 25m => "Warm",          // 20°C to 25°C
            < 30m => "Balmy",         // 25°C to 30°C
            < 35m => "Hot",           // 30°C to 35°C
            < 40m => "Sweltering",    // 35°C to 40°C
            _ => "Scorching"          // 40°C and above
        };
    }
}
