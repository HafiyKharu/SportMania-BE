namespace SportMania.Models;

public class Customer
{
    public Guid CustomerId { get; set; }
    public string UserNameDiscord { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber {get; set;} = string.Empty;

}