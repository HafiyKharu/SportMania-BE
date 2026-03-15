namespace SportMania.Models;

public class PlanDetails
{
    public Guid PlanDetailsId { get; set; }
    public string Value { get; set; } = string.Empty;
    public Guid PlanId { get; set; }
    public virtual Plan? Plan { get; set; }
}