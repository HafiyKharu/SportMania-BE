using SportMania.Models.Interface;

namespace SportMania.Models;

public class Plan : IHasAuditTimestamps
{
    public Guid PlanId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
    public virtual List<PlanDetails> Details { get; set; } = new List<PlanDetails>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
}