namespace CaptchaApi.Models
{
    // Represents a single recorded user attempt (mouse or touch)
    public class AccessEntry
    {
        // Core metadata
        public DateTime Timestamp { get; set; }               // When the attempt occurred
        public string? Status { get; set; }                   // "accepted" or "banned"
        public string? Reason { get; set; }                   // Reason for banning, if applicable
        public string? Ip { get; set; }                       // IP address of the user
        public string? InputType { get; set; }                // Input source: "mouse" or "touch"

        // Touch-specific metrics
        public float? VerticalScore { get; set; }             // Accuracy of vertical movement
        public int? VerticalCount { get; set; }               // Number of vertical motions
        public float? TotalVerticalMovement { get; set; }     // Total vertical distance
        public float? AvgSpeed { get; set; }                  // Average interaction speed
        public float? StdSpeed { get; set; }                  // Speed deviation
        public int? AccelerationChanges { get; set; }         // Count of acceleration shifts

        // Mouse-specific metrics
        public double? MaxSpeed { get; set; }                 // Maximum mouse speed
        public double? LastSpeed { get; set; }                // Final speed before submission
        public double? SpeedStability { get; set; }           // Consistency of mouse movement

        // Shared interaction data
        public int? MovementTime { get; set; }                // Total duration in ms
        public List<float>? SpeedSeries { get; set; }         // List of recorded speeds

        // Result from behavior analysis and ML model
        public string? BehaviorType { get; set; }             // "human" or "robot"
        public float? MlScore { get; set; }                   // Score from ML evaluation

        // Client and session info
        public string? UserAgent { get; set; }                // User's browser/device info
        public string? PageUrl { get; set; }                  // Originating page
        public string? AttemptId { get; set; }                // Unique identifier for this attempt
        public List<int>? BoxIndexes { get; set; }            // Visual CAPTCHA box selections

        // Additional computed metrics
        public float? DecelerationRate { get; set; }          // Speed slowdown rate
        public float? SpeedVariance { get; set; }             // Statistical variance in speed
    }
}
