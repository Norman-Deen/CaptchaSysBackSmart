using CaptchaApi.Controllers;
using CaptchaApi.ML;
using System;

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

            // Log when a new attempt is received for traceability
            Console.WriteLine("New touch attempt received");
            Console.WriteLine($"Attempt time: {now:yyyy-MM-dd HH:mm:ss}");

            // Default values in case the ML model fails
            float mlScore = 1.0f;
            string behaviorType = "human";

            try
            {
                // Build input for ML model using touch-specific features
                var input = new UserBehaviorInput
                {
                    inputType = 1, // 1 represents touch input
                    verticalScore = data.VerticalScore ?? 0,
                    verticalCount = data.VerticalCount,
                    totalVerticalMovement = data.TotalVerticalMovement,
                    avgSpeed = data.AvgSpeed,
                    stdSpeed = data.StdSpeed,
                    accelerationChanges = data.AccelerationChanges,
                    maxSpeed = 0, // not used in touch
                    lastSpeed = 0, // not used in touch
                    speedStability = 0, // not used in touch
                    movementTime = data.MovementTime,
                    decelerationRate = 0, // optional, not used currently
                    speedVariance = 0 // optional, not used currently
                };

                // Evaluate the input using the ML model
                mlScore = _evaluator.Evaluate(input);

                // Based on the score, classify the behavior
                behaviorType = mlScore >= 0.75f ? "human" : "robot";

                // Output the results to the console for monitoring
                Console.WriteLine($"ML Score: {mlScore:0.00}");
                Console.WriteLine($"Behavior: {behaviorType}");
            }
            catch (Exception ex)
            {
                // Log any error that occurs during ML evaluation
                Console.WriteLine($"ML Evaluation failed: {ex.Message}");
            }

            // Return both the classification and the ML score
            return (behaviorType, mlScore);
        }
    }
}
