using System.Globalization;
using CaptchaApi.Models;

namespace CaptchaApi.Services;

public static class LogService
{
    // Path to the log file that will store all access attempts in CSV format
    private static readonly string logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "access-log.csv");

    // Checks if a given IP address has been previously banned
    public static async Task<bool> IsIpBanned(string ip)
    {
        if (!File.Exists(logPath)) return false;

        var lines = await File.ReadAllLinesAsync(logPath);
        return lines.Any(line => line.Contains(ip) && line.Contains("banned"));
    }

    // Records a new access attempt in the CSV log file
    public static async Task AddAttempt(AccessEntry logEntry)
    {
        try
        {
            // Ensure the directory for the log file exists
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

            bool fileExists = File.Exists(logPath);

            // If the log file does not exist, create it and write the header row
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
                Console.WriteLine("Log file created with column headers.");
            }

            // Calculate deceleration rate based on the last few speed values
            double? decelerationRate = null;
            if (logEntry.SpeedSeries != null && logEntry.SpeedSeries.Count > 0)
            {
                var recent = logEntry.SpeedSeries.TakeLast(5).ToList();
                var avg = recent.Average();
                var last = logEntry.LastSpeed ?? 0;
                if (avg > 0)
                    decelerationRate = (avg - last) / avg;
            }

            // Calculate variance in the speed data to understand behavior stability
            double? speedVariance = null;
            if (logEntry.SpeedSeries != null && logEntry.SpeedSeries.Count > 1)
            {
                var mean = logEntry.SpeedSeries.Average();
                speedVariance = logEntry.SpeedSeries.Average(s => Math.Pow(s - mean, 2));
            }

            // Create a CSV line from the log entry and calculated values
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

            // Write the constructed line into the CSV file
            await File.AppendAllTextAsync(logPath, csvLine + Environment.NewLine);
            Console.WriteLine("Log entry successfully written to file.");
        }
        catch (Exception ex)
        {
            // If something goes wrong, write the error message to the console
            Console.WriteLine("Error while writing to log: " + ex.Message);
        }
    }

    // Utility function to safely quote CSV values, including escaping inner quotes
    private static string Quote(string? value)
    {
        return $"\"{value?.Replace("\"", "\"\"") ?? ""}\"";
    }
}
