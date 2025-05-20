using CaptchaApi.Models;
using System;

namespace CaptchaApi.Services;

public class BehaviorAnalyzer
{
    public static (string BehaviorType, float MlScore) AnalyzeIpBehavior(CaptchaData data, string ip)
    {
        var now = DateTime.Now;

        Console.WriteLine($"New attempt from IP: {ip}");
        Console.WriteLine($"Attempt time: {now:yyyy-MM-dd HH:mm:ss}");

        float mlScore = 1.0f; // نعطيه قيمة مبدئية آمنة
        string behaviorType = "human";
        int suspiciousScore = 0;

        // ✅ تحقق من الحظر بناءً على ملف السجل فقط
        if (LogService.IsIpBanned(ip).Result)
        {
            Console.WriteLine("❌ This IP is already banned from log file.");
            return ("banned", mlScore);
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

        // ✅ تخفيض النقاط المشبوهة إذا كانت ML قوية
        if (suspiciousScore == 1 && mlScore > 0.95f)
            suspiciousScore = 0;

        if (mlScore > 0.98f && suspiciousScore >= 2)
            suspiciousScore--;

        // ✅ تصنيف السلوك
        if (suspiciousScore >= 2)
            behaviorType = "robot";
        else if (suspiciousScore == 1)
            behaviorType = "uncertain";
        else
            behaviorType = "human";

        Console.WriteLine($"🤝 Adjusted suspiciousScore (after ML check): {suspiciousScore}");
        Console.WriteLine($"✔️ Final behaviorType: {behaviorType}");

        // ✅ إذا كان السلوك خطير، نعيده كـ روبوت – ستتم إضافته للسجل لاحقًا في CaptchaController
        if (behaviorType == "robot")
        {
            Console.WriteLine("⚠️ Behavior is too suspicious. Marked as robot.");
            return ("robot", mlScore);
        }

        Console.WriteLine("✅ Behavior is normal, access granted.");
        return (behaviorType, mlScore);
    }
}
