using SportMania.Models;
using SportMania.Models.Requests;
using SportMania.Models.Responses;

namespace SportMania.Services.Interface
{
    public interface ITransactionService
    {
        Task<(bool IsSuccess, string Result)> InitiatePaymentAsync(RequestTransaction req, string returnUrl, CancellationToken cancellationToken = default);
        Task<Transaction> ProcessPaymentCallbackAsync(Guid transactionId, string statusId, CancellationToken cancellationToken = default);
        Task<TransactionViewResponse?> GetTransactionForViewAsync(Guid transactionId, CancellationToken cancellationToken = default);
    }
}