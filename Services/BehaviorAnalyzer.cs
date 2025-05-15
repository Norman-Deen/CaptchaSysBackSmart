using CaptchaApi.Models;

namespace CaptchaApi.Services;

public class BehaviorAnalyzer
{
    private static Dictionary<string, UserBehaviorRecord> records = new();

    public static (string BehaviorType, float MlScore) AnalyzeIpBehavior(CaptchaData data, string ip)
    {
        var now = DateTime.Now;

        Console.WriteLine($"New attempt from IP: {ip}");
        Console.WriteLine($"Attempt time: {now:yyyy-MM-dd HH:mm:ss}");

        float mlScore = 1.0f; // نعطيه قيمة مبدئية آمنة
        string behaviorType = "human";
        int suspiciousScore = 0;

        if (!records.TryGetValue(ip, out var record))
        {
            Console.WriteLine("New IP detected, creating new record...");
            record = new UserBehaviorRecord
            {
                FirstSeen = now,
                TotalAttempts = 0,
                BadAttempts = 0,
                WarningLevel = 0
            };
        }
        else
        {
            if (record.IsBanned)
            {
                Console.WriteLine("This IP is permanently banned.");
                return ("banned", mlScore);
            }

            if (record.BannedUntil is not null && now < record.BannedUntil)
            {
                Console.WriteLine($"This IP is temporarily banned until: {record.BannedUntil}");
                return ("banned", mlScore);
            }
        }

        // ✅ تحليل النموذج باستخدام ML
        try
        {
            var input = new CaptchaApi.ML.MouseData
            {
                MaxSpeed = data.MaxSpeed,
                LastSpeed = data.LastSpeed,
                SpeedStability = data.SpeedStability,
                MovementTime = data.MovementTime
            };

            mlScore = CaptchaApi.ML.ModelEvaluator.PredictScore(input);
            Console.WriteLine($"🧠 ML Score: {mlScore.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}");
            Console.WriteLine($"ML score check: suspiciousScore = {suspiciousScore}");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ ML prediction failed: {ex.Message}");
        }

        // ✅ تحليل محلي
        if (data.MovementTime < 300)
            suspiciousScore++;

        if (data.SpeedStability < 0.1f)
            suspiciousScore++;

        Console.WriteLine($"📉 DecelerationRate check = {data.DecelerationRate}");
        if (data.DecelerationRate > 0.05f && data.DecelerationRate < 0.12f)

            suspiciousScore++;


        if (data.SpeedSeries != null && data.SpeedSeries.Count > 0)
        {
            float max = data.SpeedSeries.Max();
            float min = data.SpeedSeries.Min();

            if (max - min < 0.5f)
                suspiciousScore++;
        }


        // ✅ بديل لاحقًا: تصنيف مباشر فقط حسب ML Score
        // if (mlScore < 0.5f)
        //     behaviorType = "robot";
        // else if (mlScore < 0.9f)
        //     behaviorType = "uncertain";
        // else
        //     behaviorType = "human";


        if (suspiciousScore == 1 && mlScore > 0.95f)
            suspiciousScore = 0;

        if (mlScore > 0.98f && suspiciousScore >= 2)
            suspiciousScore--;


        if (suspiciousScore >= 2)
            behaviorType = "robot";
        else if (suspiciousScore == 1)
            behaviorType = "uncertain";
        else
            behaviorType = "human";

        Console.WriteLine($"🤝 Adjusted suspiciousScore (after ML check): {suspiciousScore}");
        Console.WriteLine($"✔️ Final behaviorType: {behaviorType}");

        // ✅ تحديث البيانات
        record.TotalAttempts++;
        record.LastSeen = now;
        record.LastBehaviorType = behaviorType;

        if (behaviorType is "uncertain" or "robot")
            record.BadAttempts++;

        int warningLevel = record.BadAttempts * 2;
        if (data.DecelerationRate < 0.2f)
            warningLevel++;

        record.WarningLevel = warningLevel;

        Console.WriteLine($"Total attempts: {record.TotalAttempts}");
        Console.WriteLine($"Suspicious attempts: {record.BadAttempts}");
        Console.WriteLine($"Analyzed behavior: {behaviorType}");
        Console.WriteLine($"Warning level: {record.WarningLevel}");

        if (warningLevel >= 8)
        {
            record.IsBanned = true;
            Console.WriteLine("Permanent ban applied to this user.");
            records[ip] = record;
            return ("banned", mlScore);
        }

        if (warningLevel >= 4)
        {
            record.BannedUntil = now.AddMinutes(10);
            Console.WriteLine($"Temporary ban applied for 10 minutes until: {record.BannedUntil}");
            records[ip] = record;
            return ("banned", mlScore);
        }

        Console.WriteLine("Behavior is normal, access granted.");
        records[ip] = record;
        return (behaviorType, mlScore);
    }
}
