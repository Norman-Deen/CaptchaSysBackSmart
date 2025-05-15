namespace CaptchaApi.Models;

public class AccessEntry
{
    public DateTime Timestamp { get; set; }
    public string? Status { get; set; }
    public string? Reason { get; set; }
    public string? Ip { get; set; }

    public double? MaxSpeed { get; set; }
    public double? LastSpeed { get; set; }
    public double? SpeedStability { get; set; }

    //public double? DecelerationRate { get; set; }
   // public int? DecelerationRatio { get; set; }
    //public string? MovementPattern { get; set; }

    public int? MovementTime { get; set; }
    public List<float>? SpeedSeries { get; set; }

    public string? BehaviorType { get; set; }



    public float? MlScore { get; set; }

    public string? UserAgent { get; set; }
    public string? PageUrl { get; set; }
    public string? AttemptId { get; set; }
    public List<int>? BoxIndexes { get; set; }

}
