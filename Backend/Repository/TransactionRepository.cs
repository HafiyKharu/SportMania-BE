using Microsoft.EntityFrameworkCore;
using SportMania.Data;
using SportMania.Models;
using SportMania.Repository.Interface;

namespace SportMania.Repository;

public class TransactionRepository (ApplicationDbContext _context) : ITransactionRepository
{
    public async Task<Transaction> CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        transaction.TransactionId = Guid.NewGuid();
        await _context.Transactions.AddAsync(transaction, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return transaction;
    }

    public async Task DeleteTransactionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Transactions.FindAsync(new object[] { id }, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (transaction != null)
        {
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Customer)
            .Include(t => t.Plan)
            .Include(t => t.Key)
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Transaction?> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Customer)
            .Include(t => t.Plan)
            .Include(t => t.Key)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TransactionId == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UpdateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _context.Entry(transaction).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}