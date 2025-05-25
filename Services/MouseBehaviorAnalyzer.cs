using System;
using System.Linq;
using CaptchaApi.ML;

namespace CaptchaApi.Services
{
    // This class analyzes mouse behavior and determines if it belongs to a human or a robot
    public static class MouseBehaviorAnalyzer
    {
        // Reference to the machine learning evaluator that calculates the classification score
        private static readonly ScoreBasedEvaluator _evaluator = new();

        // Main method to analyze mouse data and return classification and score
        public static (string BehaviorType, float MlScore) Analyze(CaptchaData data, string ip)
        {
            var now = DateTime.Now;

            // Print the cleaned IP address and timestamp to console for debugging or logging
            Console.WriteLine($"{ip.Replace("::ffff:", "")}");
            Console.WriteLine($"{now:yyyy-MM-dd HH:mm:ss}");

            // Default classification and score in case something goes wrong
            float mlScore = 1.0f;
            string behaviorType = "human";

            // Extra calculations from speedSeries if available
            float avgSpeed = 0;
            float stdSpeed = 0;
            int accelerationChanges = 0;
            float decelerationRate = 0;
            float speedVariance = 0;

            if (data.SpeedSeries != null && data.SpeedSeries.Count > 0)
            {
                // avgSpeed
                avgSpeed = (float)data.SpeedSeries.Average();

                // stdSpeed
                var mean = data.SpeedSeries.Average();
                stdSpeed = (float)Math.Sqrt(data.SpeedSeries.Average(s => Math.Pow(s - mean, 2)));

                // accelerationChanges
                for (int i = 1; i < data.SpeedSeries.Count - 1; i++)
                {
                    var prev = data.SpeedSeries[i - 1];
                    var curr = data.SpeedSeries[i];
                    var next = data.SpeedSeries[i + 1];

                    if ((curr > prev && curr > next) || (curr < prev && curr < next))
                        accelerationChanges++;
                }

                // decelerationRate
                var recent = data.SpeedSeries.TakeLast(5).ToList();
                var recentAvg = recent.Average();
                var last = data.LastSpeed;
                if (recentAvg > 0)
                    decelerationRate = (float)((recentAvg - last) / recentAvg);

                // speedVariance
                speedVariance = (float)data.SpeedSeries.Average(s => Math.Pow(s - mean, 2));
            }

            try
            {
                // Create the input object for the machine learning model
                // These values are specifically chosen for mouse data
                var input = new UserBehaviorInput
                {
                    inputType = 0, // 0 = mouse input
                    verticalScore = 0, // not used for mouse
                    verticalCount = 0,
                    totalVerticalMovement = 0,
                    avgSpeed = avgSpeed,
                    stdSpeed = stdSpeed,
                    accelerationChanges = accelerationChanges,
                    maxSpeed = data.MaxSpeed,
                    lastSpeed = data.LastSpeed,
                    speedStability = (float)data.SpeedStability,
                    movementTime = data.MovementTime,
                    decelerationRate = decelerationRate,
                    speedVariance = speedVariance
                };

                // Call the ML model to evaluate the user behavior
                mlScore = _evaluator.Evaluate(input);

                // Threshold-based decision: classify as "human" or "robot"
                behaviorType = mlScore >= 0.75f ? "human" : "robot";

                // Output score and decision for transparency and debugging
                Console.WriteLine($"ML Score: {mlScore:0.00}");
                Console.WriteLine($"Behavior: {behaviorType}\n");
            }
            catch (Exception ex)
            {
                // Catch and print any error that might occur during ML evaluation
                Console.WriteLine($"ML prediction failed: {ex.Message}");
            }

            // Inject computed values back into the original data object
            data.AvgSpeed = avgSpeed;
            data.StdSpeed = stdSpeed;
            data.AccelerationChanges = accelerationChanges;
            data.DecelerationRate = decelerationRate;
            data.SpeedVariance = speedVariance;

            // Return the final classification result and score
            return (behaviorType, mlScore);
        }
    }
}