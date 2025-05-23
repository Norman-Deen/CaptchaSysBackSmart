namespace CaptchaApi.Models
{
    // Represents a single user attempt with both mouse and touch metrics
    public class AccessEntry
    {
        public DateTime Timestamp { get; set; } // When the attempt occurred
        public string? Status { get; set; } // Result of the attempt: "accepted" or "banned"
        public string? Reason { get; set; } // Reason for banning, if applicable
        public string? Ip { get; set; } // User's IP address
        public string? InputType { get; set; } // Type of input: "mouse" or "touch"

        // Touch input metrics
        public float? VerticalScore { get; set; } // Calculated vertical score (touch only)
        public int? VerticalCount { get; set; } // Number of vertical movements
        public float? TotalVerticalMovement { get; set; } // Sum of all vertical movement distances
        public float? AvgSpeed { get; set; } // Average speed of user interaction
        public float? StdSpeed { get; set; } // Standard deviation of speed
        public int? AccelerationChanges { get; set; } // Number of changes in acceleration

        // Mouse input metrics
        public double? MaxSpeed { get; set; } // Maximum speed recorded during the attempt
        public double? LastSpeed { get; set; } // Last speed value before submission
        public double? SpeedStability { get; set; } // Consistency of mouse speed

        public int? MovementTime { get; set; } // Total time of interaction in milliseconds
        public List<float>? SpeedSeries { get; set; } // Raw list of recorded speeds over time

        public string? BehaviorType { get; set; } // Result from behavior analysis: "human" or "robot"
        public float? MlScore { get; set; } // Score from machine learning evaluation

        public string? UserAgent { get; set; } // User's browser and system details
        public string? PageUrl { get; set; } // The page where the attempt was made
        public string? AttemptId { get; set; } // Unique ID generated per attempt
        public List<int>? BoxIndexes { get; set; } // Used box indices (for visual CAPTCHA)
    }
}
