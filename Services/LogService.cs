using System.Globalization;
using CaptchaApi.Models;

namespace CaptchaApi.Services;

// Handles all logging functionality for user attempts
public static class LogService
{
    // Path to the CSV log file where access attempts are saved
    private static readonly string logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "access-log.csv");

    // Checks if the given IP address has a previous banned entry in the log
    public static async Task<bool> IsIpBanned(string ip)
    {
        if (!File.Exists(logPath)) return false;

        var lines = await File.ReadAllLinesAsync(logPath);
        return lines.Any(line => line.Contains(ip) && line.Contains("banned"));
    }

    // Writes a new access attempt into the CSV log file
    public static async Task AddAttempt(AccessEntry logEntry)
    {
        try
        {
            // Make sure the Logs directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            bool fileExists = File.Exists(logPath);

            // If log file doesn't exist, create it and write the column headers
            if (!fileExists)
            {
                var headers = string.Join(",", new[]
                {
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

            // Construct a CSV-formatted line from the access entry data
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

                logEntry.DecelerationRate?.ToString("0.00", CultureInfo.InvariantCulture) ?? "",
                logEntry.SpeedVariance?.ToString("0.00", CultureInfo.InvariantCulture) ?? "",
                logEntry.MlScore?.ToString("0.00", CultureInfo.InvariantCulture) ?? "",

                Quote(logEntry.PageUrl),
                Quote(logEntry.UserAgent),
                logEntry.BoxIndexes != null
                    ? Quote(string.Join(";", logEntry.BoxIndexes))
                    : "",
                Quote(logEntry.AttemptId)
            });

            // Append the line to the CSV file
            await File.AppendAllTextAsync(logPath, csvLine + Environment.NewLine);
            Console.WriteLine("Log entry successfully written to file.");
        }
        catch (Exception ex)
        {
            // Handle any error that occurs during file write
            Console.WriteLine("Error while writing to log: " + ex.Message);
        }
    }

    // Escapes and wraps a CSV field in double quotes to avoid formatting issues
    private static string Quote(string? value)
    {
        return $"\"{value?.Replace("\"", "\"\"") ?? ""}\"";
    }
}
