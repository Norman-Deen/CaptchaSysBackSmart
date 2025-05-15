namespace CaptchaApi.Models;

public class UserBehaviorRecord
{
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public int TotalAttempts { get; set; }
    public int BadAttempts { get; set; }
    public string? LastBehaviorType { get; set; }
    public string? LastMovementPattern { get; set; }
    public int WarningLevel { get; set; }
    public bool IsBanned { get; set; }
    public DateTime? BannedUntil { get; set; }
}

