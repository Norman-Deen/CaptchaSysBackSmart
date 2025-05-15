using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace CaptchaApi.Controllers;



[ApiController]
[Route("api/[controller]")]
public class LogController : ControllerBase
{
    private readonly string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "access-log.csv");



    [HttpGet]
    public IActionResult GetLog()
    {
        if (!System.IO.File.Exists(logFilePath))
            return NotFound("Log file not found.");

        var lines = System.IO.File.ReadAllLines(logFilePath);
        return Ok(lines);
    }






// DELETE /api/log/{index}
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