---
description: "Generate Entity Framework Core model/entity classes and DbContext configuration with relationships, constraints, and audit tracking for .NET 10"
argument-hint: "Entity name and properties (e.g., 'Payment entity with Id, Amount, CustomerId, Status, CreationDate')"
agent: ".NET 10 API Services"
---

# Data Object & Entity Framework Core Generator

You are a specialist at designing database models and Entity Framework Core configurations for .NET 10 applications.

## Task

Generate complete database entity classes and related Entity Framework Core configuration. Include:

1. **Entity class** with properties, navigation properties, and relationships
2. **Data types** optimized for storage and performance
3. **Constraints** (required fields, max length, precision, etc.)
4. **Navigation properties** for related entities
5. **Audit timestamps** (CreatedAt, ModifiedAt) if applicable
6. **Fluent API configuration** in `OnModelCreating()`
7. **Foreign keys** and relationship setup
8. **Index definitions** for commonly queried columns
9. **Composite keys** or alternate key definitions if needed
10. **Value object patterns** where appropriate

## Requirements

- Use C# 13+ features: records for immutable DTOs, nullable reference types
- Entity classes should be plain old CLR objects (POCOs) with public properties
- Navigation properties should use `virtual` for lazy loading (or IEnumerable for explicit loading)
- Configure one-to-many, many-to-many, and one-to-one relationships correctly
- Use `decimal` for monetary amounts with appropriate precision (typically `decimal(18, 2)`)
- Include soft delete support (IsDeleted flag) if required
- Audit tracking for CreatedAt, ModifiedAt, CreatedBy, ModifiedBy
- Use fluent API in `OnModelCreating()` for detailed configuration

## Output Format

Provide:
1. **Entity class definition** with all properties and navigation properties
2. **DbSet declaration** for `ApplicationDbContext`
3. **Fluent API configuration** block for `OnModelCreating()`

Example:
```csharp
/// <summary>
/// Represents a payment transaction in the system.
/// </summary>
public class Payment : IHasAuditTimestamps
{
    /// <summary>Unique identifier for the payment.</summary>
    public int Id { get; set; }

    /// <summary>Foreign key to the customer who made the payment.</summary>
    [Required]
    public int CustomerId { get; set; }

    /// <summary>The payment amount in base currency (USD, MYR, etc.).</summary>
    [Required]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    /// <summary>Current status of the payment (Pending, Completed, Failed, Refunded).</summary>
    [Required]
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>Optional external reference (e.g., payment gateway transaction ID).</summary>
    [StringLength(100)]
    public string? ExternalReference { get; set; }

    /// <summary>Notes or metadata about the payment.</summary>
    [StringLength(500)]
    public string? Notes { get; set; }

    /// <summary>When the payment was created (UTC).</summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When the payment was last modified (UTC).</summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>Navigation property to the related customer.</summary>
    public virtual Customer Customer { get; set; } = null!;

    /// <summary>Navigation collection to related transactions.</summary>
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

// In ApplicationDbContext.cs:
public DbSet<Payment> Payments { get; set; }

// In OnModelCreating():
modelBuilder.Entity<Payment>(entity =>
{
    entity.HasKey(p => p.Id);

    entity.Property(p => p.CustomerId)
        .IsRequired();

    entity.Property(p => p.Amount)
        .HasPrecision(18, 2)
        .IsRequired();

    entity.Property(p => p.Status)
        .HasConversion(s => s.ToString(), s => Enum.Parse<PaymentStatus>(s))
        .IsRequired();

    entity.Property(p => p.ExternalReference)
        .HasMaxLength(100);

    entity.Property(p => p.Notes)
        .HasMaxLength(500);

    entity.Property(p => p.CreatedAt)
        .IsRequired()
        .HasDefaultValueSql("GETUTCDATE()");

    // Foreign key relationship
    entity.HasOne(p => p.Customer)
        .WithMany(c => c.Payments)
        .HasForeignKey(p => p.CustomerId)
        .OnDelete(DeleteBehavior.Restrict);

    // Index for common queries
    entity.HasIndex(p => p.CustomerId);
    entity.HasIndex(p => p.Status);
    entity.HasIndex(p => p.CreatedAt);
});

// Define enum
public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Refunded = 3
}
```
