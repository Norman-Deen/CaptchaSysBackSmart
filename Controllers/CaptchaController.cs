using Microsoft.AspNetCore.Mvc;
using CaptchaApi.Models;
using CaptchaApi.Services;
using System.Text.Json;

namespace CaptchaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CaptchaController : ControllerBase
{
    private static int count = 0;

    [HttpPost]
    public async Task<IActionResult> ReceiveCaptcha([FromBody] JsonElement data)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"-Received Captcha Data, Input: {++count}\n");
        Console.ResetColor();

        string ip = HttpContext.Connection.RemoteIpAddress?.ToString() switch
        {
            "::1" => "127.0.0.1",
            null => "unknown",
            var realIp => realIp
        };

        // ✅ في حال كشف روبوت مباشر من الواجهة
        if (data.TryGetProperty("mode", out var modeProperty) && modeProperty.GetString() == "robot-detected")
        {
            Console.WriteLine("🤖 Robot detected!");
            return await FakeBoxAnalyzer.HandleRobotDetected(data, ip);
        }

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var captchaData = JsonSerializer.Deserialize<CaptchaData>(data, options);

        if (captchaData == null)
        {
            Console.WriteLine("❌ captchaData is null");
            return BadRequest(new { success = false, status = "invalid" });
        }

        string finalStatus = captchaData?.Status == "banned" ? "banned" : "accepted";

        // ✅ في حال كان محظور مسبقًا
        if (await LogService.IsIpBanned(ip))
        {
            Console.WriteLine("❌ This IP was banned before. Blocking again.");
            var behaviorType = "robot"; // أو "banned" إذا بتحب تمييزه

            await LogService.AddAttempt(new AccessEntry
            {
                Timestamp = DateTime.Now,
                Ip = ip,
                Status = "banned",
                Reason = "Blocked due to previous ban",
                MaxSpeed = captchaData?.MaxSpeed ?? 0,
                LastSpeed = captchaData?.LastSpeed ?? 0,
                SpeedStability = captchaData?.SpeedStability ?? 0,
                MovementTime = captchaData?.MovementTime ?? 0,
                SpeedSeries = captchaData?.SpeedSeries,
                UserAgent = captchaData?.UserAgent,
                PageUrl = captchaData?.PageUrl,
                BehaviorType = behaviorType,
                MlScore = null, // محظور بدون تحليل
                AttemptId = Guid.NewGuid().ToString()
            });

            return Content("{\"success\":false,\"status\":\"banned\"}", "application/json");
        }

        // ✅ تحليل السلوك AI
        var (behaviorTypeFinal, mlScore) = BehaviorAnalyzer.AnalyzeIpBehavior(captchaData, ip);

        // ✅ طباعة البيانات
        Console.WriteLine("📦 Data Received:\n");
        Console.WriteLine(JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));

        await LogService.AddAttempt(new AccessEntry
        {
            Timestamp = DateTime.Now,
            Ip = ip,
            Status = behaviorTypeFinal == "robot" ? "banned" : "accepted",
            BehaviorType = behaviorTypeFinal,
            MlScore = mlScore,

            Reason = captchaData?.Reason,
            MaxSpeed = captchaData?.MaxSpeed ?? 0,
            LastSpeed = captchaData?.LastSpeed ?? 0,
            SpeedStability = captchaData?.SpeedStability ?? 0,
            MovementTime = captchaData?.MovementTime ?? 0,
            SpeedSeries = captchaData?.SpeedSeries,
            PageUrl = captchaData?.PageUrl,
            UserAgent = captchaData?.UserAgent,
            AttemptId = Guid.NewGuid().ToString()
        });

        if (behaviorTypeFinal == "robot")
            return Content("{\"success\":false,\"status\":\"banned\"}", "application/json");

        return Content($"{{\"success\":true,\"status\":\"{behaviorTypeFinal}\"}}", "application/json");
    }
}
