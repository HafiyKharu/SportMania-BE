namespace SportMania.Models.Responses;

/// <summary>
/// Response model for transaction retrieval with one-time key visibility behavior.
/// </summary>
public class TransactionViewResponse
{
    /// <summary>
    /// Unique identifier of the transaction.
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// Unique identifier of the customer associated with the transaction.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Unique identifier of the purchased plan.
    /// </summary>
    public Guid PlanId { get; set; }

    /// <summary>
    /// Optional identifier of the generated key record.
    /// </summary>
    public Guid? KeyId { get; set; }

    /// <summary>
    /// Discord guild identifier associated with the generated key and transaction.
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    /// Charged amount displayed in currency-formatted string form.
    /// </summary>
    public string Amount { get; set; } = string.Empty;

    /// <summary>
    /// Payment status value such as <c>Pending</c>, <c>Success</c>, or <c>Failed</c>.
    /// </summary>
    public string PaymentStatus { get; set; } = string.Empty;

    /// <summary>
    /// Optional bill code returned by the payment provider.
    /// </summary>
    public string? BillCode { get; set; }

    /// <summary>
    /// Indicates whether the transaction key has been viewed at least once.
    /// </summary>
    public bool IsKeyViewed { get; set; }

    /// <summary>
    /// License key value. Present only on the first successful read; null afterwards.
    /// </summary>
    public string? LicenseKey { get; set; }
}