---
description: "Use when implementing data access with repositories and Entity Framework Core. Covers query patterns, async operations, LINQ best practices, and database context management in .NET 10."
applyTo: ["Repository/**/*.cs", "Data/**/*.cs"]
---

# Repository & Data Access Guidelines (.NET 10)

## Repository Pattern

- **Class naming**: `{Entity}Repository` (e.g., `PaymentRepository`, `KeyRepository`)
- **Interface naming**: `I{Entity}Repository` (e.g., `IPaymentRepository`)
- **Single responsibility**: Each repository manages one entity type
- **Stateless design**: No instance state except the injected `DbContext`

```csharp
public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> FindAsync(Expression<Func<Payment, bool>> predicate, CancellationToken cancellationToken = default);
    Task AddAsync(Payment entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Payment entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
```

**Rules**:
- Return `Task<T?>` for single entity queries (may be null)
- Return `Task<IEnumerable<T>>` for collections
- All methods accept `CancellationToken` parameter
- Use `Expression<Func<T, bool>>` for flexible filtering in Find methods

## Entity Framework Core Integration

- Inject `ApplicationDbContext` (not `DbContext` directly)
- Always use `AsNoTracking()` for read-only queries to improve performance
- Use `SaveChangesAsync()` explicitly only in repository methods, not in services
- Leverage automatic change tracking for updates and deletes

```csharp
public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Payment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task AddAsync(Payment entity, CancellationToken cancellationToken = default)
    {
        await _context.Payments.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Payment entity, CancellationToken cancellationToken = default)
    {
        _context.Payments.Update(entity);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Payments.FindAsync(new object[] { id }, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (entity != null)
        {
            _context.Payments.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
```

## LINQ Query Patterns

- **Eager loading**: Use `.Include()` or `.ThenInclude()` for related entities when needed
- **Filtering**: Always apply predicates in LINQ queries, not in-memory
- **Sorting**: Use `.OrderBy()`, `.OrderByDescending()` at query time
- **Pagination**: Implement with `.Skip()` and `.Take()`
- **Projection**: Use `.Select()` to map to DTOs before materializing

```csharp
public async Task<IEnumerable<TransactionDto>> GetTransactionsByCustomerAsync(
    int customerId,
    int pageNumber = 1,
    int pageSize = 10,
    CancellationToken cancellationToken = default)
{
    var pageIndex = (pageNumber - 1) * pageSize;

    return await _context.Transactions
        .Where(t => t.CustomerId == customerId)
        .OrderByDescending(t => t.CreatedAt)
        .Skip(pageIndex)
        .Take(pageSize)
        .Select(t => new TransactionDto
        {
            Id = t.Id,
            Amount = t.Amount,
            Status = t.Status,
            CreatedAt = t.CreatedAt
        })
        .ToListAsync(cancellationToken)
        .ConfigureAwait(false);
}
```

## Async & Cancellation

- All query execution must be async: `.ToListAsync()`, `.FirstOrDefaultAsync()`, `.CountAsync()`
- Always pass `CancellationToken` to async LINQ methods
- Use `ConfigureAwait(false)` on every `await`
- Avoid blocking calls like `.Result` or `.Wait()`

## DbContext Configuration

- Configure entity mappings in `ApplicationDbContext.OnModelCreating()`
- Define relationships, keys, constraints at the DbContext level
- Use fluent API for complex configurations; data annotations for simple ones
- Include navigation properties for related entities

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Payment> Payments { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Customer)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

## Transaction Management

- Use explicit transactions only for multi-step operations that must succeed atomically
- Let `SaveChangesAsync()` handle implicit transactions for single operations
- Rollback on exceptions; log before rethrowing

```csharp
public async Task TransferPaymentAsync(int fromId, int toId, decimal amount, CancellationToken cancellationToken = default)
{
    using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
    try
    {
        // Deduct from source
        var source = await _context.Payments.FindAsync(new object[] { fromId }, cancellationToken: cancellationToken).ConfigureAwait(false);
        source.Amount -= amount;

        // Add to destination
        var dest = await _context.Payments.FindAsync(new object[] { toId }, cancellationToken: cancellationToken).ConfigureAwait(false);
        dest.Amount += amount;

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogError(ex, "Payment transfer failed");
        throw;
    }
}
```

## Audit & Timestamps

- Implement `IHasAuditTimestamps` interface for entities that need created/modified tracking
- Automatically set `CreatedAt` on insert; update `ModifiedAt` on every change
- Override `SaveChangesAsync()` in `ApplicationDbContext` to handle audit fields

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var now = DateTime.UtcNow;

    foreach (var entry in ChangeTracker.Entries<IHasAuditTimestamps>())
    {
        if (entry.State == EntityState.Added)
            entry.Entity.CreatedAt = now;

        if (entry.State == EntityState.Modified)
            entry.Entity.ModifiedAt = now;
    }

    return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
}
```

## Testing Considerations

- Use in-memory database for unit tests: `new DbContextOptions<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("TestDb").Options)`
- Mock the `ApplicationDbContext` for service layer unit tests
- Integration tests use real database with test seeds and cleanup
