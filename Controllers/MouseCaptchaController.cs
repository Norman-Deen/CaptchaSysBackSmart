using CaptchaApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using CaptchaApi.Models;

[ApiController]
[Route("api/box")]
public class MouseCaptchaController : ControllerBase
{
    // A simple counter to track how many requests have been received
    private static int count = 0;

    [HttpPost]
    public async Task<IActionResult> Analyze([FromBody] JsonElement data)
    {
        // Clear console for each request (useful during testing)
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Received Mouse Captcha: {++count}\n");
        Console.ResetColor();

        // Extract the IP address of the incoming request
        string ip = HttpContext.Request.Headers.ContainsKey("X-Forwarded-For")
      ? HttpContext.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0]
      : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";


        // Handle robot-detected signal from frontend (manual override)
        if (data.TryGetProperty("mode", out var modeProperty) && modeProperty.GetString() == "robot-detected")
        {
            Console.WriteLine("Robot detected (frontend signal)");

            string reason = data.TryGetProperty("reason", out var reasonElem) ? reasonElem.GetString() ?? "Three fake clicks detected" : "Three fake clicks detected";
            string attemptId = data.TryGetProperty("attemptId", out var idElem) ? idElem.GetString() ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString();
            string userAgent = data.TryGetProperty("userAgent", out var uaElem) ? uaElem.GetString() ?? "unknown" : "unknown";
            string pageUrl = data.TryGetProperty("pageUrl", out var urlElem) ? urlElem.GetString() ?? "unknown" : "unknown";

            List<int>? boxIndexes = null;
            if (data.TryGetProperty("boxIndexes", out var boxArrayElem) && boxArrayElem.ValueKind == JsonValueKind.Array)
            {
                boxIndexes = boxArrayElem.EnumerateArray().Select(x => x.GetInt32()).ToList();
            }

            // Log the attempt as a robot
            await LogService.AddAttempt(new AccessEntry
            {
                Timestamp = DateTime.Now,
                Ip = ip,
                InputType = "mouse",
                Status = "banned",
                Reason = reason,
                AttemptId = attemptId,
                UserAgent = userAgent,
                PageUrl = pageUrl,
                BoxIndexes = boxIndexes,
                BehaviorType = "robot"
            });

            return Content("{\"success\":false,\"status\":\"banned\"}", "application/json");
        }

        // Deserialize the incoming JSON into a CaptchaData object
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var captchaData = JsonSerializer.Deserialize<CaptchaData>(data, options);

        if (captchaData == null)
        {
            Console.WriteLine("captchaData is null");
            return BadRequest(new { success = false, status = "invalid" });
        }

        // Analyze behavior using mouse-specific features
        (string behavior, float score) = MouseBehaviorAnalyzer.Analyze(captchaData, ip);

        // Log raw input data for review
        Console.WriteLine("Raw Mouse Data:");
        Console.WriteLine(JsonSerializer.Serialize(captchaData, new JsonSerializerOptions { WriteIndented = true }));

        // Save the attempt to the log
        await LogService.AddAttempt(new AccessEntry
        {
            Timestamp = DateTime.Now,
            Ip = ip,
            InputType = "mouse",
            Status = behavior == "robot" ? "banned" : "accepted",
            BehaviorType = behavior,
            MlScore = score,
            Reason = captchaData.Reason,
            MaxSpeed = captchaData.MaxSpeed,
            LastSpeed = captchaData.LastSpeed,
            SpeedStability = captchaData.SpeedStability,
            MovementTime = captchaData.MovementTime,
            SpeedSeries = captchaData.SpeedSeries,
            PageUrl = captchaData.PageUrl,
            UserAgent = captchaData.UserAgent,
            AttemptId = Guid.NewGuid().ToString()
        });

        // Return the result to the client
        if (behavior == "robot")
            return Content("{\"success\":false,\"status\":\"banned\"}", "application/json");

        return Content($"{{\"success\":true,\"status\":\"{behavior}\"}}", "application/json");
    }
}

// Represents the structure of mouse interaction data sent from the frontend
public class CaptchaData
{
    public float MaxSpeed { get; set; } // Maximum mouse speed during the interaction
    public float LastSpeed { get; set; } // Last recorded speed before submitting
    public float SpeedStability { get; set; } // How stable the speed was across the interaction
    public int MovementTime { get; set; } // Total duration of movement in milliseconds
    public List<float>? SpeedSeries { get; set; } // List of all recorded speed values
    public string? AttemptId { get; set; } // Unique identifier for this attempt
    public string? UserAgent { get; set; } // Browser and device information
    public string? BehaviorType { get; set; } // Classification label, if provided by client
    public string? Status { get; set; } // Result label, if provided by client
    public string? PageUrl { get; set; } // The URL where the captcha appeared
    public string? Reason { get; set; } // Reason for classification or banning
    public float DecelerationRate { get; set; } // How quickly the movement slowed down
}
