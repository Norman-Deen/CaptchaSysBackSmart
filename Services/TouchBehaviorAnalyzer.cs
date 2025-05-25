using CaptchaApi.Controllers;
using CaptchaApi.ML;
using System;
using System.Linq;

namespace CaptchaApi.Services
{
    // This class analyzes touch-based input behavior to determine if it comes from a human or a robot
    public static class TouchBehaviorAnalyzer
    {
        // Instance of the machine learning evaluator used to compute the behavior score
        private static readonly ScoreBasedEvaluator _evaluator = new();

        // Main method to analyze a touch input and return both the classification and ML score
        public static (string BehaviorType, float MlScore) Analyze(TouchCaptchaData data)
        {
            var now = DateTime.Now;

            Console.WriteLine("New touch attempt received");
            Console.WriteLine($"Attempt time: {now:yyyy-MM-dd HH:mm:ss}");

            // Default values
            float mlScore = 1.0f;
            string behaviorType = "human";

            // Prepare values and override incomplete ones from frontend
            float avgSpeed = data.AvgSpeed;
            float stdSpeed = data.StdSpeed;
            int accelerationChanges = data.AccelerationChanges;
            float speedVariance = 0;
            float decelerationRate = 0;

            if (data.SpeedSeries != null && data.SpeedSeries.Count > 0)
            {
                var speeds = data.SpeedSeries;
                avgSpeed = (float)speeds.Average();
                var mean = avgSpeed;

                stdSpeed = (float)Math.Sqrt(speeds.Average(s => Math.Pow(s - mean, 2)));
                speedVariance = (float)speeds.Average(s => Math.Pow(s - mean, 2));

                // Acceleration changes
                accelerationChanges = 0;
                for (int i = 1; i < speeds.Count - 1; i++)
                {
                    var prev = speeds[i - 1];
                    var curr = speeds[i];
                    var next = speeds[i + 1];
                    if ((curr > prev && curr > next) || (curr < prev && curr < next))
                        accelerationChanges++;
                }

                // Deceleration rate
                var last = speeds.Last();
                var recent = speeds.TakeLast(5).ToList();
                var recentAvg = recent.Average();
                if (recentAvg > 0)
                    decelerationRate = (recentAvg - last) / recentAvg;
            }

            try
            {
                //Console.WriteLine($"Injected values after analysis: " +
                //    $"AvgSpeed={avgSpeed}, " +
                //    $"StdSpeed={stdSpeed}, " +
                //    $"AccelerationChanges={accelerationChanges}, " +
                //    $"DecelerationRate={decelerationRate}, " +
                //    $"SpeedVariance={speedVariance}");

                // Build input for ML model using all computed features
                var input = new UserBehaviorInput
                {
                    inputType = 1,
                    verticalScore = data.VerticalScore ?? 0,
                    verticalCount = data.VerticalCount,
                    totalVerticalMovement = data.TotalVerticalMovement,
                    avgSpeed = avgSpeed,
                    stdSpeed = stdSpeed,
                    accelerationChanges = accelerationChanges,
                    maxSpeed = 0,
                    lastSpeed = 0,
                    speedStability = 0,
                    movementTime = data.MovementTime,
                    decelerationRate = decelerationRate,
                    speedVariance = speedVariance
                };

                mlScore = _evaluator.Evaluate(input);
                behaviorType = mlScore >= 0.75f ? "human" : "robot";

                Console.WriteLine($"ML Score: {mlScore:0.00}");
                Console.WriteLine($"Behavior: {behaviorType}");

                // Inject the final computed values back into the data object
                data.AvgSpeed = avgSpeed;
                data.StdSpeed = stdSpeed;
                data.AccelerationChanges = accelerationChanges;
                data.DecelerationRate = decelerationRate;
                data.SpeedVariance = speedVariance;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"ML Evaluation failed: {ex.Message}");
            }

            return (behaviorType, mlScore);
        }
    }
}
