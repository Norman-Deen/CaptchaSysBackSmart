using CaptchaApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using CaptchaApi.Models;

namespace CaptchaApi.Controllers;

[ApiController]
[Route("api/slider")]
public class TouchCaptchaController : ControllerBase
{
    // Counter to keep track of how many captchas have been received
    private static int attemptCounter = 0;

    [HttpPost]
    public async Task<IActionResult> ReceiveTouchData([FromBody] JsonElement rawData)
    {
        // Clear the console and print the current attempt number
        Console.Clear();
        Console.WriteLine($"- Received Captcha Data, Input: {++attemptCounter}\n");

        // Get the IP address of the client
        string ip = HttpContext.Connection.RemoteIpAddress?.ToString() switch
        {
            "::1" => "127.0.0.1",
            null => "unknown",
            var realIp => realIp
        };

        DateTime now = DateTime.Now;

        Console.WriteLine($"New attempt from IP: {ip}");
        Console.WriteLine($"Attempt time: {now:yyyy-MM-dd HH:mm:ss}");

        // Check if the frontend has explicitly flagged the input as a robot
        if (rawData.TryGetProperty("mode", out var modeProp) && modeProp.GetString() == "robot-detected")
        {
            // Print robot detection info for debugging
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                Mode = "robot-detected",
                Reason = rawData.GetProperty("reason").GetString(),
                PageUrl = rawData.GetProperty("pageUrl").GetString(),
                UserAgent = rawData.GetProperty("userAgent").GetString()
            }, new JsonSerializerOptions { WriteIndented = true }));

            // Log the attempt as banned
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

            // Return banned status to the frontend
            return Ok(new { success = true, status = "banned" });
        }

        // Attempt to parse the JSON into a typed object
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<TouchCaptchaData>(rawData, options);

        if (data == null)
        {
            // Log and return invalid if parsing fails
            Console.WriteLine("Failed to parse touch data.");
            return BadRequest(new { success = false, status = "invalid" });
        }

        // Analyze user behavior to determine if it's human or robot
        string behaviorType = TouchBehaviorAnalyzer.Analyze(data);
        float mlScore = 1.0f; // Default ML score (can be updated later with a real ML model)

        Console.WriteLine($"ML Score: {mlScore:0.00}");
        Console.WriteLine($"Final behaviorType: {behaviorType}");

        // Print received data for debugging
        Console.WriteLine("Data Received:\n");
        Console.WriteLine(JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));

        // Save the result to the log file
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

            // Values used to analyze user behavior (touch-specific)
            VerticalScore = data.VerticalScore,
            VerticalCount = data.VerticalCount,
            TotalVerticalMovement = data.TotalVerticalMovement,
            AvgSpeed = data.AvgSpeed,
            StdSpeed = data.StdSpeed,
            AccelerationChanges = data.AccelerationChanges,
            MovementTime = data.MovementTime,
            SpeedSeries = data.SpeedSeries
        });

        // Return the final result
        return Ok(new { success = true, status = behaviorType });
    }
}

// Class representing the structure of touch interaction data sent from frontend
public class TouchCaptchaData
{
    public float AvgSpeed { get; set; }
    public float StdSpeed { get; set; }
    public int AccelerationChanges { get; set; }
    public int MovementTime { get; set; }
    public int VerticalCount { get; set; }
    public float? VerticalScore { get; set; }
    public float TotalVerticalMovement { get; set; }
    public List<float>? SpeedSeries { get; set; }
    public string? PageUrl { get; set; }
    public string? UserAgent { get; set; }
}

// Class responsible for analyzing touch behavior and returning a classification
public static class TouchBehaviorAnalyzer
{
    public static string Analyze(TouchCaptchaData data)
    {
        int score = 0;

        // These are basic heuristics to help decide if the behavior is suspicious
        if (data.MovementTime < 300) score++;
        if (data.StdSpeed < 0.05f) score++;
        if (data.AccelerationChanges < 5) score++;
        if (data.VerticalCount < 2 && data.TotalVerticalMovement < 2) score++;

        return score >= 2 ? "banned" : "human";
    }
}
