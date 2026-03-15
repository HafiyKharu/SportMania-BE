namespace SportMania.Models.Requests;

public class RequestTransaction
{
  public string Email { get; set; } = "";
  public Guid PlanId { get; set; }
  public string PhoneNumber { get; set; } = "";
}