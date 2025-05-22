using CaptchaApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CaptchaApi.Controllers;

[ApiController]
[Route("api/slider")]
public class TouchCaptchaController : ControllerBase
{
    private static int attemptCounter = 0;

    [HttpPost]
    public async Task<IActionResult> ReceiveTouchData([FromBody] JsonElement rawData)
    {
        Console.Clear();
        Console.WriteLine($"- Received Captcha Data, Input: {++attemptCounter}\n");

        string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        DateTime now = DateTime.Now;

        Console.WriteLine($"New attempt from IP: {ip}");
        Console.WriteLine($"Attempt time: {now:yyyy-MM-dd HH:mm:ss}");

        // For robot-detected
        if (rawData.TryGetProperty("mode", out var modeProp) && modeProp.GetString() == "robot-detected")
        {
           
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                Mode = "robot-detected",
                Reason = rawData.GetProperty("reason").GetString(),
                PageUrl = rawData.GetProperty("pageUrl").GetString(),
                UserAgent = rawData.GetProperty("userAgent").GetString()
            }, new JsonSerializerOptions { WriteIndented = true }));

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


        // ✅ فك تشفير البيانات إلى TouchCaptchaData
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<TouchCaptchaData>(rawData, options);

        if (data == null)
        {
            Console.WriteLine("❌ Failed to parse touch data.");
            return BadRequest(new { success = false, status = "invalid" });
        }

        // ✅ تحليل السلوك
        string behaviorType = TouchBehaviorAnalyzer.Analyze(data);
        float mlScore = 1.0f; // في حال أضفت ML لاحقًا

        Console.WriteLine($"🧠 ML Score: {mlScore:0.00}");
        Console.WriteLine($"✔️ Final behaviorType: {behaviorType}");

        Console.WriteLine("📦 Data Received:\n");
        Console.WriteLine(JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));

        // ✅ تسجيل المحاولة
        await LogService.AddAttempt(new AccessEntry
        {
            Timestamp = now,
            Ip = ip,
            InputType = "touch", // ← مهم جداً

            Status = behaviorType == "banned" ? "banned" : "accepted",
            BehaviorType = behaviorType,
            MlScore = mlScore,
            Reason = null,
            UserAgent = data.UserAgent,
            PageUrl = data.PageUrl,
            AttemptId = Guid.NewGuid().ToString(),

            // ✅ القيم الخاصة بمنطق اللمس
            VerticalScore = data.VerticalScore,
            VerticalCount = data.VerticalCount,
            TotalVerticalMovement = data.TotalVerticalMovement,
            AvgSpeed = data.AvgSpeed,
            StdSpeed = data.StdSpeed,
            AccelerationChanges = data.AccelerationChanges,
            MovementTime = data.MovementTime,
            SpeedSeries = data.SpeedSeries
        });


        return Ok(new { success = true, status = behaviorType });
    }
}



// 📦 بيانات اللمس القادمة من الواجهة
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

// 🧠 منطق تحليل السلوك البسيط
public static class TouchBehaviorAnalyzer
{
    public static string Analyze(TouchCaptchaData data)
    {
        int score = 0;

        if (data.MovementTime < 300) score++;
        if (data.StdSpeed < 0.05f) score++;
        if (data.AccelerationChanges < 5) score++;
        if (data.VerticalCount < 2 && data.TotalVerticalMovement < 2) score++;

        return score >= 2 ? "banned" : "human";
    }
}
