using SportMania.Models;

namespace SportMania.Repository.Interface;

public interface ITransactionRepository
{
    Task<IEnumerable<Transaction>> GetAllTransactionsAsync(CancellationToken cancellationToken = default);
    Task<Transaction?> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Transaction?> GetTransactionByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Transaction> CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task UpdateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task DeleteTransactionAsync(Guid id, CancellationToken cancellationToken = default);
}