using Xfer.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Xfer.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/xfer", "application/json")]
public class SampleDataController : ControllerBase {
    private readonly ILogger<SampleDataController> _logger;
    private static readonly List<SampleData> _sampleDataStore = new();

    public SampleDataController(ILogger<SampleDataController> logger) {
        _logger = logger;
    }

    /// <summary>
    /// Get sample data demonstrating XferLang serialization capabilities
    /// </summary>
    /// <returns>Sample data with various .NET types</returns>
    [HttpGet]
    [ProducesResponseType(typeof(SampleData), 200)]
    public ActionResult<SampleData> GetSampleData() {
        var sample = new SampleData {
            Name = "Alice Johnson",
            Age = 30,
            TimeSpan = new TimeSpan(28, 11, 43, 56), // 28 days, 11:43:56
            TimeOnly = new TimeOnly(11, 43, 56),
            DateTime = new DateTime(2021, 10, 31, 12, 34, 56),
            TestEnum = TestEnum.Pretty,
            Salary = 75000.50m,
            IsActive = true,
            Tags = new List<string> { "employee", "senior", "developer" },
            Metadata = new Dictionary<string, object> {
                { "department", "Engineering" },
                { "startDate", new DateTime(2020, 1, 15) },
                { "skillLevel", 8.5 },
                { "hasRemoteAccess", true }
            }
        };

        _logger.LogInformation("Returning sample data for {Name}", sample.Name);
        return Ok(sample);
    }

    /// <summary>
    /// Echo back the posted data to demonstrate XferLang deserialization
    /// </summary>
    /// <param name="data">The sample data to echo</param>
    /// <returns>The same data that was posted</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SampleData), 200)]
    [ProducesResponseType(400)]
    public ActionResult<SampleData> PostSampleData([FromBody] SampleData data) {
        if (!ModelState.IsValid) {
            _logger.LogWarning("Invalid model state: {Errors}", 
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Received and echoing back data for {Name}", data.Name);
        
        // Store for demonstration purposes
        _sampleDataStore.Add(data);
        
        return Ok(data);
    }

    /// <summary>
    /// Get all stored sample data
    /// </summary>
    /// <returns>List of all sample data that has been posted</returns>
    [HttpGet("all")]
    [ProducesResponseType(typeof(List<SampleData>), 200)]
    public ActionResult<List<SampleData>> GetAllSampleData() {
        _logger.LogInformation("Returning {Count} stored sample data items", _sampleDataStore.Count);
        return Ok(_sampleDataStore);
    }

    /// <summary>
    /// Clear all stored sample data
    /// </summary>
    /// <returns>No content</returns>
    [HttpDelete("all")]
    [ProducesResponseType(204)]
    public IActionResult ClearAllSampleData() {
        var count = _sampleDataStore.Count;
        _sampleDataStore.Clear();
        _logger.LogInformation("Cleared {Count} sample data items", count);
        return NoContent();
    }

    /// <summary>
    /// Test endpoint to demonstrate various data types in XferLang
    /// </summary>
    /// <returns>Complex data structure with nested objects and collections</returns>
    [HttpGet("complex")]
    [ProducesResponseType(200)]
    public ActionResult<object> GetComplexData() {
        var complexData = new {
            Message = "XferLang Complex Data Demo",
            Timestamp = DateTime.UtcNow,
            Numbers = new[] { 1, 2, 3, 5, 8, 13, 21 },
            NestedObject = new {
                Level1 = new {
                    Level2 = new {
                        Value = "Deep nesting works!"
                    }
                }
            },
            NullableValues = new {
                HasValue = (int?)42,
                IsNull = (string?)null,
                DefaultDecimal = (decimal?)null
            },
            Enums = Enum.GetValues<TestEnum>(),
            BooleanTests = new {
                TrueValue = true,
                FalseValue = false
            }
        };

        _logger.LogInformation("Returning complex demonstration data");
        return Ok(complexData);
    }
}
