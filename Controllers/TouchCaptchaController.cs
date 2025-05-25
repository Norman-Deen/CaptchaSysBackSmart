using CaptchaApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using CaptchaApi.Models;

namespace CaptchaApi.Controllers;

[ApiController]
[Route("api/slider")]
// Controller to handle touch-based CAPTCHA submissions
public class TouchCaptchaController : ControllerBase
{
    // Counter to track how many touch captchas have been received
    private static int attemptCounter = 0;

    [HttpPost]
    // Receives touch data, analyzes behavior, and logs the result
    public async Task<IActionResult> ReceiveTouchData([FromBody] JsonElement rawData)
    {
        Console.Clear();
        Console.WriteLine($"- Received Captcha Data, Input: {++attemptCounter}\n");

        // Extract client IP address
        string ip = HttpContext.Request.Headers.ContainsKey("X-Forwarded-For")
            ? HttpContext.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0]
            : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        DateTime now = DateTime.Now;
        Console.WriteLine($"New attempt from IP: {ip}");
        Console.WriteLine($"Attempt time: {now:yyyy-MM-dd HH:mm:ss}");

        // Handle explicit robot detection signal from frontend
        if (rawData.TryGetProperty("mode", out var modeProp) && modeProp.GetString() == "robot-detected")
        {
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                Mode = "robot-detected",
                Reason = rawData.GetProperty("reason").GetString(),
                PageUrl = rawData.GetProperty("pageUrl").GetString(),
                UserAgent = rawData.GetProperty("userAgent").GetString()
            }, new JsonSerializerOptions { WriteIndented = true }));

            // Save flagged attempt to log
            await LogService.AddAttempt(new AccessEntry
            {
                Timestamp = DateTime.Now,
                Ip = ip,
                InputType = "touch",
                Status = "banned",
                Reason = rawData.GetProperty("reason").GetString(),
                BehaviorType = "robot",
                UserAgent = rawData.GetProperty("userAgent").GetString(),
                PageUrl = rawData.GetProperty("pageUrl").GetString(),
                AttemptId = Guid.NewGuid().ToString()
            });

            return Ok(new { success = true, status = "banned" });
        }

        // Deserialize touch data from incoming JSON
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<TouchCaptchaData>(rawData, options);

        if (data == null)
        {
            Console.WriteLine("Failed to parse touch data.");
            return BadRequest(new { success = false, status = "invalid" });
        }

        // Analyze the touch interaction and get classification
        (string behaviorType, float mlScore) = TouchBehaviorAnalyzer.Analyze(data);

        Console.WriteLine($"ML Score: {mlScore:0.00}");
        Console.WriteLine($"Final behaviorType: {behaviorType}");

        Console.WriteLine("Data Received:\n");
        Console.WriteLine(JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));

        // Save analyzed data to log
        await LogService.AddAttempt(new AccessEntry
        {
            Timestamp = now,
            Ip = ip,
            InputType = "touch",
            Status = behaviorType == "banned" ? "banned" : "accepted",
            BehaviorType = behaviorType,
            MlScore = mlScore,
            Reason = null,
            UserAgent = data.UserAgent,
            PageUrl = data.PageUrl,
            AttemptId = Guid.NewGuid().ToString(),
            VerticalScore = data.VerticalScore,
            VerticalCount = data.VerticalCount,
            TotalVerticalMovement = data.TotalVerticalMovement,
            AvgSpeed = data.AvgSpeed,
            StdSpeed = data.StdSpeed,
            AccelerationChanges = data.AccelerationChanges,
            MovementTime = data.MovementTime,
            SpeedSeries = data.SpeedSeries,
            DecelerationRate = data.DecelerationRate,
            SpeedVariance = data.SpeedVariance
        });

        return Ok(new { success = true, status = behaviorType });
    }
}

// Represents the structure of touch interaction data sent from the frontend
public class TouchCaptchaData
{
    public float AvgSpeed { get; set; }                      // Average interaction speed
    public float StdSpeed { get; set; }                      // Speed standard deviation
    public int AccelerationChanges { get; set; }             // Number of speed shifts
    public int MovementTime { get; set; }                    // Total duration in milliseconds
    public int VerticalCount { get; set; }                   // Number of vertical touches
    public float? VerticalScore { get; set; }                // Scoring accuracy for vertical swipes
    public float TotalVerticalMovement { get; set; }         // Total vertical movement distance
    public List<float>? SpeedSeries { get; set; }            // Raw list of speed samples
    public string? PageUrl { get; set; }                     // URL of the page using the CAPTCHA
    public string? UserAgent { get; set; }                   // Browser/device info

    // Calculated features (populated in analyzer)
    public float DecelerationRate { get; set; }              // Decline rate of movement
    public float SpeedVariance { get; set; }                 // Statistical variance in speeds
}
