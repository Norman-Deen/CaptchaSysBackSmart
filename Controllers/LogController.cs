using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace CaptchaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogController : ControllerBase
{
    // Full path to the CSV file that stores all access attempts
    private readonly string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "access-log.csv");

    // GET /api/log
    // Returns all lines from the log file
    [HttpGet]
    public IActionResult GetLog()
    {
        if (!System.IO.File.Exists(logFilePath))
        {
            // If file doesn't exist, return an empty array instead of null or error
            return Ok(new string[] { });
        }

        var lines = System.IO.File.ReadAllLines(logFilePath);
        return Ok(lines);
    }

    // DELETE /api/log/{index}
    // Deletes a specific line from the log file based on its index
    [HttpDelete("{index}")]
    public IActionResult DeleteLine(int index)
    {
        if (!System.IO.File.Exists(logFilePath))
            return NotFound("Log file not found.");

        var lines = System.IO.File.ReadAllLines(logFilePath).ToList();

        if (index < 0 || index >= lines.Count)
            return BadRequest("Invalid index.");

        lines.RemoveAt(index);
        System.IO.File.WriteAllLines(logFilePath, lines);

        return Ok(new { message = $"Line {index} deleted." });
    }
}
