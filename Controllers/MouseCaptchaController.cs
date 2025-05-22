using CaptchaApi.Services;

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;



[ApiController]
[Route("api/box")]
public class MouseCaptchaController : ControllerBase
{
    private static int count = 0;

    [HttpPost]
    public async Task<IActionResult> Analyze([FromBody] JsonElement data)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"- Received Mouse Captcha Input: {++count}\n");
        Console.ResetColor();

        string ip = HttpContext.Connection.RemoteIpAddress?.ToString() switch
        {
            "::1" => "127.0.0.1",
            null => "unknown",
            var realIp => realIp
        };

        // Robot logic
        if (data.TryGetProperty("mode", out var modeProperty) && modeProperty.GetString() == "robot-detected")
        {
            Console.WriteLine("🤖 Robot detected (frontend signal)");

            string reason = data.TryGetProperty("reason", out var reasonElem) ? reasonElem.GetString() ?? "Three fake clicks detected" : "Three fake clicks detected";
            string attemptId = data.TryGetProperty("attemptId", out var idElem) ? idElem.GetString() ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString();
            string userAgent = data.TryGetProperty("userAgent", out var uaElem) ? uaElem.GetString() ?? "unknown" : "unknown";
            string pageUrl = data.TryGetProperty("pageUrl", out var urlElem) ? urlElem.GetString() ?? "unknown" : "unknown";

            List<int>? boxIndexes = null;
            if (data.TryGetProperty("boxIndexes", out var boxArrayElem) && boxArrayElem.ValueKind == JsonValueKind.Array)
            {
                boxIndexes = boxArrayElem.EnumerateArray().Select(x => x.GetInt32()).ToList();
            }

            await LogService.AddAttempt(new AccessEntry
            {
                Timestamp = DateTime.Now,
                Ip = ip,
                InputType = "mouse", // ✅ تحديد النوع من داخل السيرفر
                Status = "banned",
                Reason = reason,
                AttemptId = attemptId,
                UserAgent = userAgent,
                PageUrl = pageUrl,
                BoxIndexes = boxIndexes,
                BehaviorType = "robot"
            });

            Console.WriteLine("⛔ Status: BANNED");
            return Content("{\"success\":false,\"status\":\"banned\"}", "application/json");
        }

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var captchaData = JsonSerializer.Deserialize<CaptchaData>(data, options);

        if (captchaData == null)
        {
            Console.WriteLine("❌ captchaData is null");
            return BadRequest(new { success = false, status = "invalid" });
        }

       

        (string behavior, float score) = MouseBehaviorAnalyzer.Analyze(captchaData, ip);

        Console.WriteLine("📦 Raw Mouse Data:");
        Console.WriteLine(JsonSerializer.Serialize(captchaData, new JsonSerializerOptions { WriteIndented = true }));

        await LogService.AddAttempt(new AccessEntry
        {
            Timestamp = DateTime.Now,
            Ip = ip,
            InputType = "mouse", // ✅ تحديد النوع من داخل السيرفر
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

        if (behavior == "robot")
            return Content("{\"success\":false,\"status\":\"banned\"}", "application/json");

        return Content($"{{\"success\":true,\"status\":\"{behavior}\"}}", "application/json");
    }
}

public class AccessEntry
{
    public DateTime Timestamp { get; set; }
    public string? Status { get; set; }
    public string? Reason { get; set; }
    public string? Ip { get; set; }

    // 🔄 جديد: تمييز نوع الإدخال
    public string? InputType { get; set; } // "mouse" أو "touch"

    // 🔢 خصائص لمنطق اللمس
    public float? VerticalScore { get; set; }
    public int? VerticalCount { get; set; }
    public float? TotalVerticalMovement { get; set; }
    public float? AvgSpeed { get; set; }
    public float? StdSpeed { get; set; }
    public int? AccelerationChanges { get; set; }

    // 🐭 خصائص الماوس
    public double? MaxSpeed { get; set; }
    public double? LastSpeed { get; set; }
    public double? SpeedStability { get; set; }

    public int? MovementTime { get; set; }
    public List<float>? SpeedSeries { get; set; }

    public string? BehaviorType { get; set; }
    public float? MlScore { get; set; }

    public string? UserAgent { get; set; }
    public string? PageUrl { get; set; }
    public string? AttemptId { get; set; }
    public List<int>? BoxIndexes { get; set; }
}


public class CaptchaData
{
    public float MaxSpeed { get; set; }
    public float LastSpeed { get; set; }
    public float SpeedStability { get; set; }
    public int MovementTime { get; set; }
    public List<float>? SpeedSeries { get; set; }
    public string? AttemptId { get; set; }
    public string? UserAgent { get; set; }
    public string? BehaviorType { get; set; }
    public string? Status { get; set; }
    public string? PageUrl { get; set; }
    public string? Reason { get; set; }
    public float DecelerationRate { get; set; }
}
