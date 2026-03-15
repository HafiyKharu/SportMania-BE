using Microsoft.EntityFrameworkCore;
using SportMania.Data;
using SportMania.Models;
using SportMania.Repository.Interface;

namespace SportMania.Repository;

public class TransactionRepository (ApplicationDbContext _context) : ITransactionRepository
{
    public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
    {
        transaction.TransactionId = Guid.NewGuid();
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task DeleteTransactionAsync(Guid id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction != null)
        {
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
    {
        return await _context.Transactions
            .Include(t => t.Customer)
            .Include(t => t.Plan)
            .Include(t => t.Key)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Transaction?> GetTransactionByIdAsync(Guid id)
    {
        return await _context.Transactions
            .Include(t => t.Customer)
            .Include(t => t.Plan)
            .Include(t => t.Key)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TransactionId == id);
    }

    public async Task UpdateTransactionAsync(Transaction transaction)
    {
        _context.Entry(transaction).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}