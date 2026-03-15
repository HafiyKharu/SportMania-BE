using System.Text.Json.Serialization;

namespace SportMania.Models;

public class Key
{
    public Guid KeyId { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public ulong GuildId { get; set; }
    public Guid PlanId { get; set; }
    public ulong? RedeemedByUserId { get; set; }
    public DateTime? RedeemedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int DurationDays { get; set; }
    public bool IsActive { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime CreatedAt { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Plan? Plan { get; set; }
}