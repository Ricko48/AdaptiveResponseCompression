using AdaptiveResponseCompression.Server.Attributes;
using AdaptiveResponseCompression.Server.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ApiServerDemo.Controllers;

// sets response compression method (Adaptive, Standard, None) for actions in this controller
[ResponseCompression(ResponseCompressionMethod.Adaptive)]
[ApiController]
[Route("api/[controller]")]
public class TestDataController : ControllerBase
{
    /// <summary>
    /// Gets the JSON file of a specified size.
    /// </summary>
    /// <remarks>
    /// All sample JSON files were generated using <a href="https://www.mockaroo.com/">Mockaroo</a> tool.
    /// 
    /// Supported file sizes:
    /// - 1B
    /// - 500B
    /// - 1KB
    /// - 5KB
    /// - 10KB
    /// - 50KB
    /// - 100KB
    /// - 500KB
    /// - 1MB
    /// - 5MB
    /// - 10MB
    /// - 50MB
    /// - 100MB
    /// 
    /// Sample request:
    /// <code>
    /// GET /api/testData/json/10KB
    /// </code>   
    /// </remarks>
    /// <returns>File content of specified size</returns>
    /// <response code="200">Returns File content of specified size.</response>
    /// /// <response code="404">No file was found for specified size.</response>
    [HttpGet("json/{size}")]
    public IActionResult GetJsonAsync(string size)
    {
        Stream stream;

        try
        {
            stream = new FileStream($"Data/Json/data_{size}.json", FileMode.Open, FileAccess.Read);
        }
        catch (FileNotFoundException)
        {
            return NotFound($"Json file with name data_{size}.json was not found");
        }
        
        return File(stream, "application/json");
    }
}