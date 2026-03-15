using SportMania.Models.Interface;

namespace SportMania.Models;

public class Transaction : IHasAuditTimestamps
{
    public Guid TransactionId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid PlanId { get; set; }
    public Guid? KeyId { get; set; }
    public ulong GuildId { get; set; }
    public string Amount { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = "Pending";
    public string? BillCode { get; set; }

    public Customer Customer { get; set; } = null!;
    public Plan Plan { get; set; } = null!;
    public Key? Key { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
}