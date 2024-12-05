using Xfer.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Xfer.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SampleDataController : ControllerBase {
    private readonly ILogger<SampleDataController> _logger;

    public SampleDataController(ILogger<SampleDataController> logger) {
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<SampleData> GetSampleData() {
        var sample = new SampleData {
            Name = "Alice",
            Age = 30,
            TimeSpan = new TimeSpan(28, 11, 43, 56),
            TimeOnly = new TimeOnly(11, 43, 56),
            DateTime = new DateTime(2021, 10, 31, 12, 34, 56),
        };
        return Ok(sample); // Automatically serialized to Xfer
    }

    [HttpPost]
    public ActionResult<SampleData> PostSampleData([FromBody] SampleData data) {
        return Ok(data); // Echo the received data
    }
}
