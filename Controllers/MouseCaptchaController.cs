using CaptchaApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using CaptchaApi.Models;

[ApiController]
[Route("api/box")]
// Controller responsible for handling mouse-based captcha submissions
public class MouseCaptchaController : ControllerBase
{
    // Counter for tracking the number of received mouse captchas
    private static int count = 0;

    [HttpPost]
    // Handles incoming mouse interaction data and analyzes user behavior
    public async Task<IActionResult> Analyze([FromBody] JsonElement data)
    {
        // Clear console output for clean testing view
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Received Mouse Captcha: {++count}\n");
        Console.ResetColor();

        // Get client IP address (handles proxies and direct connections)
        string ip = HttpContext.Request.Headers.ContainsKey("X-Forwarded-For")
            ? HttpContext.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0]
            : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Manually triggered robot detection from frontend
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

            // Log banned attempt
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

        // Parse incoming JSON into CaptchaData object
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var captchaData = JsonSerializer.Deserialize<CaptchaData>(data, options);

        if (captchaData == null)
        {
            Console.WriteLine("captchaData is null");
            return BadRequest(new { success = false, status = "invalid" });
        }

        // Analyze the user's mouse behavior and classify it
        (string behavior, float score) = MouseBehaviorAnalyzer.Analyze(captchaData, ip);

        // Print raw data to console for review
        Console.WriteLine("Raw Mouse Data:");
        Console.WriteLine(JsonSerializer.Serialize(captchaData, new JsonSerializerOptions { WriteIndented = true }));

        // Save all analyzed data to the log file
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
            AttemptId = Guid.NewGuid().ToString(),
            AvgSpeed = captchaData.AvgSpeed,
            StdSpeed = captchaData.StdSpeed,
            AccelerationChanges = captchaData.AccelerationChanges,
            SpeedVariance = captchaData.SpeedVariance,
            DecelerationRate = captchaData.DecelerationRate
        });

        // Return classification result to the client
        if (behavior == "robot")
            return Content("{\"success\":false,\"status\":\"banned\"}", "application/json");

        return Content($"{{\"success\":true,\"status\":\"{behavior}\"}}", "application/json");
    }
}

// Data model for mouse interaction values received from the frontend
public class CaptchaData
{
    public float MaxSpeed { get; set; }                 // Highest speed during interaction
    public float LastSpeed { get; set; }                // Final speed before submission
    public float SpeedStability { get; set; }           // Consistency of speed throughout
    public int MovementTime { get; set; }               // Total interaction duration in ms
    public List<float>? SpeedSeries { get; set; }       // Raw list of recorded speeds
    public string? AttemptId { get; set; }              // Unique attempt identifier
    public string? UserAgent { get; set; }              // Browser/device info
    public string? BehaviorType { get; set; }           // Optional override from frontend
    public string? Status { get; set; }                 // Optional override status
    public string? PageUrl { get; set; }                // Source page of interaction
    public string? Reason { get; set; }                 // Reason for flagging (if any)
    public float DecelerationRate { get; set; }         // Rate of speed decline
    public float AvgSpeed { get; set; }                 // Calculated average speed
    public float StdSpeed { get; set; }                 // Standard deviation of speed
    public int AccelerationChanges { get; set; }        // Number of speed changes
    public float SpeedVariance { get; set; }            // Variability in speed
}
