using System.Text.Json.Serialization;

namespace SportMania.Models;

public class PlanRoleMapping
{
    public Guid MappingId { get; set; }
    public ulong GuildId { get; set; }
    public Guid PlanId { get; set; }
    public ulong RoleId { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime CreatedAt { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Plan? Plan { get; set; }
}