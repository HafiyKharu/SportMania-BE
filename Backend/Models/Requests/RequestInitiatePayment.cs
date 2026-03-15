namespace SportMania.Models.Requests;

public class RequestInitiatePayment
{
    public string Email { get; set; } = string.Empty;
    public Guid PlanId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
}
