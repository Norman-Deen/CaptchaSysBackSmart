using System.Globalization;

namespace CaptchaApi.Services;

public static class LogService
{
    private static readonly string logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "access-log.csv");

    public static async Task<bool> IsIpBanned(string ip)
    {
        if (!File.Exists(logPath)) return false;

        var lines = await File.ReadAllLinesAsync(logPath);
        return lines.Any(line => line.Contains(ip) && line.Contains("banned"));
    }

    public static async Task AddAttempt(AccessEntry logEntry)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!); // ✅ يضمن وجود المجلد

            bool fileExists = File.Exists(logPath);

            if (!fileExists)
            {
                var headers = string.Join(",", new[] {
                "timestamp", "ip", "inputType", "status", "reason", "behaviorType",
                "verticalScore", "verticalCount", "totalVerticalMovement",
                "avgSpeed", "stdSpeed", "accelerationChanges",
                "maxSpeed", "lastSpeed", "speedStability", "movementTime",
                "speedSeries", "decelerationRate", "speedVariance", "mlScore",
                "pageUrl", "userAgent", "boxIndexes", "attemptId"
            });


                await File.AppendAllTextAsync(logPath, headers + Environment.NewLine);
                Console.WriteLine("📄 Created log file with headers.");
            }

            // ✅ حساب decelerationRate
            double? decelerationRate = null;
            if (logEntry.SpeedSeries != null && logEntry.SpeedSeries.Count > 0)
            {
                var recent = logEntry.SpeedSeries.TakeLast(5).ToList();
                var avg = recent.Average();
                var last = logEntry.LastSpeed ?? 0;
                if (avg > 0)
                    decelerationRate = (avg - last) / avg;
            }

            // ✅ حساب speedVariance
            double? speedVariance = null;
            if (logEntry.SpeedSeries != null && logEntry.SpeedSeries.Count > 1)
            {
                var mean = logEntry.SpeedSeries.Average();
                speedVariance = logEntry.SpeedSeries.Average(s => Math.Pow(s - mean, 2));
            }


            // ✅ توليد سطر CSV
            var csvLine = string.Join(",", new[]
             {
                Quote(logEntry.Timestamp.ToString("s")),
                Quote(logEntry.Ip),
                Quote(logEntry.InputType),
                Quote(logEntry.Status),
                Quote(logEntry.Reason),
                Quote(logEntry.BehaviorType),

                logEntry.VerticalScore?.ToString("0.00", CultureInfo.InvariantCulture) ?? "",
                logEntry.VerticalCount?.ToString() ?? "",
                logEntry.TotalVerticalMovement?.ToString("0.00", CultureInfo.InvariantCulture) ?? "",

                logEntry.AvgSpeed?.ToString("0.00", CultureInfo.InvariantCulture) ?? "",
                logEntry.StdSpeed?.ToString("0.00", CultureInfo.InvariantCulture) ?? "",
                logEntry.AccelerationChanges?.ToString() ?? "",

                logEntry.MaxSpeed?.ToString("0.00", CultureInfo.InvariantCulture) ?? "",
                logEntry.LastSpeed?.ToString("0.00", CultureInfo.InvariantCulture) ?? "",
                logEntry.SpeedStability?.ToString("0.00", CultureInfo.InvariantCulture) ?? "",
                logEntry.MovementTime?.ToString() ?? "",

                logEntry.SpeedSeries != null
                    ? Quote(string.Join(";", logEntry.SpeedSeries.Select(s => s.ToString("0.00", CultureInfo.InvariantCulture))))
                    : "",

                decelerationRate?.ToString("0.00", CultureInfo.InvariantCulture) ?? "",
                speedVariance?.ToString("0.00", CultureInfo.InvariantCulture) ?? "",
                logEntry.MlScore?.ToString("0.00", CultureInfo.InvariantCulture) ?? "",

                Quote(logEntry.PageUrl),
                Quote(logEntry.UserAgent),
                logEntry.BoxIndexes != null
                    ? Quote(string.Join(";", logEntry.BoxIndexes))
                    : "",
                Quote(logEntry.AttemptId)
            });


            await File.AppendAllTextAsync(logPath, csvLine + Environment.NewLine);
            Console.WriteLine("\n✅ Log line written successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error writing to log: " + ex.Message);
        }
    }

    private static string Quote(string? value)
    {
        return $"\"{value?.Replace("\"", "\"\"") ?? ""}\"";
    }
}
