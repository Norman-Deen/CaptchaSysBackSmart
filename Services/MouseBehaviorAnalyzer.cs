using System;

namespace CaptchaApi.Services;

public static class MouseBehaviorAnalyzer
{
    public static (string BehaviorType, float MlScore) Analyze(CaptchaData data, string ip)
    {
        var now = DateTime.Now;

        Console.WriteLine($"New mouse attempt from IP: {ip}");
        Console.WriteLine($"Attempt time: {now:yyyy-MM-dd HH:mm:ss}");

        float mlScore = 1.0f;
        string behaviorType = "human";
        int suspiciousScore = 0;

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

        if (data.SpeedSeries is { Count: > 0 })
        {
            float max = data.SpeedSeries.Max();
            float min = data.SpeedSeries.Min();
            if (max - min < 0.5f)
                suspiciousScore++;
        }

        // ✅ تعديل النقاط المشبوهة حسب ML
        if (suspiciousScore == 1 && mlScore > 0.95f)
            suspiciousScore = 0;

        if (mlScore > 0.98f && suspiciousScore >= 2)
            suspiciousScore--;

        // ✅ تصنيف السلوك
        if (suspiciousScore >= 2)
            behaviorType = "robot";
        else if (suspiciousScore == 1)
            behaviorType = "uncertain";

        Console.WriteLine($"🤝 Adjusted suspiciousScore: {suspiciousScore}");
        Console.WriteLine($"✔️ Final behaviorType: {behaviorType}");

        return (behaviorType, mlScore);
    }
}
