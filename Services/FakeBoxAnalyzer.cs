using CaptchaApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CaptchaApi.Services;

public static class FakeBoxAnalyzer
{
    public static async Task<IActionResult> HandleRobotDetected(JsonElement data, string ip)
    {
        Console.WriteLine("\n⛔ Status: Robot");
        Console.WriteLine($"🕒 Time: {data.GetProperty("timestamp").GetString()}");
        Console.WriteLine($"🌐 IP Address: {ip}");

        // ✅ استخراج بيانات النصوص (اللي ممكن تكون null)
        string userAgent = data.TryGetProperty("userAgent", out var userAgentElem)
            ? userAgentElem.GetString() ?? "unknown"
            : "unknown";

        string pageUrl = data.TryGetProperty("pageUrl", out var pageUrlElem)
            ? pageUrlElem.GetString() ?? "unknown"
            : "unknown";

        string reason = data.TryGetProperty("reason", out var reasonElem)
            ? reasonElem.GetString() ?? "Three fake clicks detected"
            : "Three fake clicks detected";

        string attemptId = data.TryGetProperty("attemptId", out var attemptIdElem) && attemptIdElem.ValueKind == JsonValueKind.String
            ? attemptIdElem.GetString() ?? Guid.NewGuid().ToString()
            : Guid.NewGuid().ToString();

        Console.WriteLine($"🖥️ User-Agent: {userAgent}");
        Console.WriteLine($"🌍 Page URL: {pageUrl}");
        Console.WriteLine($"❗ Reason: {reason}");

        // ✅ قائمة المربعات (boxIndexes)
        List<int>? boxIndexList = null;
        if (data.TryGetProperty("boxIndexes", out var boxArrayElem) && boxArrayElem.ValueKind == JsonValueKind.Array)
        {
            boxIndexList = boxArrayElem.EnumerateArray().Select(x => x.GetInt32()).ToList();

            Console.WriteLine("📦 Box Indexes:");
            foreach (var index in boxIndexList)
                Console.WriteLine($"   - Box {index}");
        }

        // ✅ إنشاء السجل
        var entry = new AccessEntry
        {
            Timestamp = DateTime.Now,
            Ip = ip,
            Status = "banned",
            Reason = reason,
            AttemptId = attemptId,
            UserAgent = userAgent,
            PageUrl = pageUrl,
            BoxIndexes = boxIndexList,
            BehaviorType = "robot"
        };

        await LogService.AddAttempt(entry);

        Console.WriteLine("⛔ Status: BANNED");
        return new JsonResult(new { success = false, status = "banned" });
    }
}
