using Microsoft.EntityFrameworkCore;
using SportMania.Models;
using SportMania.Models.Interface;

namespace SportMania.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Plan> Plans { get; set; }
    public DbSet<PlanDetails> PlanDetails { get; set; }
    public DbSet<Key> Keys { get; set; }
    public DbSet<PlanRoleMapping> PlanRoleMappings { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IHasAuditTimestamps>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                    entry.Entity.DeletedAt = null;
                    entry.Entity.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = utcNow;
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("SportMania");

        builder.Entity<Plan>()
            .HasMany(p => p.Details)
            .WithOne(d => d.Plan)
            .HasForeignKey(d => d.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Plan>().HasQueryFilter(p => !p.IsDeleted);
        builder.Entity<Transaction>().HasQueryFilter(t => !t.IsDeleted);

        builder.Entity<Transaction>()
            .HasOne(t => t.Customer)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Transaction>()
            .HasOne(t => t.Plan)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Transaction>()
            .HasOne(t => t.Key)
            .WithMany()
            .HasForeignKey(t => t.KeyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.PhoneNumber);
        });

        builder.Entity<Key>(entity =>
        {
            entity.HasKey(e => e.KeyId);
            entity.HasIndex(e => e.LicenseKey).IsUnique();
            entity.HasIndex(e => e.GuildId);

            entity.HasOne(e => e.Plan)
                  .WithMany()
                  .HasForeignKey(e => e.PlanId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.KeyId)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("now() at time zone 'utc'");
        });

        builder.Entity<PlanRoleMapping>(entity =>
        {
            entity.HasKey(e => e.MappingId);
            entity.HasIndex(e => new { e.GuildId, e.PlanId }).IsUnique();

            entity.HasOne(e => e.Plan)
                  .WithMany()
                  .HasForeignKey(e => e.PlanId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.MappingId)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.CreatedAt)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("now() at time zone 'utc'");
        });

        // Seed Plans
        var weeklyId  = Guid.Parse("3f2c8d3e-7f22-4e54-9c2b-4a8b9d5a1a11");
        var monthlyId = Guid.Parse("0a9c7f12-1b23-4c45-8d67-1e2f3a4b5c66");
        var seasonId  = Guid.Parse("9b8a7c6d-5e4f-4a3b-9c2d-1a0b9c8d7e6f");
        var seededAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.Entity<Plan>().HasData(
            new Plan
            {
                PlanId = weeklyId,
                ImageUrl = "/Media/BatteryBar.png",
                Name = "Weekly Pass",
                Description = "Access all content for 7 days",
                Price = "RM3.00",
                Duration = "7",
                CategoryCode = "slwjpoyb",
                CreatedAt = seededAt,
                UpdatedAt = null,
                DeletedAt = null,
                IsDeleted = false
            },
            new Plan
            {
                PlanId = monthlyId,
                ImageUrl = "/Media/Star.jpeg",
                Name = "Monthly Plan",
                Description = "Full access for 30 days",
                Price = "RM10.00",
                Duration = "30",
                CategoryCode = "l2vq3l43",
                CreatedAt = seededAt,
                UpdatedAt = null,
                DeletedAt = null,
                IsDeleted = false
            },
            new Plan
            {
                PlanId = seasonId,
                ImageUrl = "/Media/crown.png",
                Name = "Season Pass",
                Description = "Premium access for the entire season",
                Price = "RM60.00",
                Duration = "90",
                CategoryCode = "bsweyxnv",
                CreatedAt = seededAt,
                UpdatedAt = null,
                DeletedAt = null,
                IsDeleted = false
            }
        );

        builder.Entity<PlanDetails>().HasData(
            new { PlanDetailsId = Guid.Parse("c1b9d1c0-2d4d-4d3b-9f3e-b5bd6f5f0001"), PlanId = weeklyId,  Value = "7 days of full access" },
            new { PlanDetailsId = Guid.Parse("c1b9d1c0-2d4d-4d3b-9f3e-b5bd6f5f0002"), PlanId = weeklyId,  Value = "HD streaming" },
            new { PlanDetailsId = Guid.Parse("c1b9d1c0-2d4d-4d3b-9f3e-b5bd6f5f0003"), PlanId = weeklyId,  Value = "Cancel anytime" },
            new { PlanDetailsId = Guid.Parse("d2c0e2d1-3e5e-4e4c-af4f-c6ce7e6e0001"), PlanId = monthlyId, Value = "30 days of full access" },
            new { PlanDetailsId = Guid.Parse("d2c0e2d1-3e5e-4e4c-af4f-c6ce7e6e0002"), PlanId = monthlyId, Value = "HD & 4K streaming" },
            new { PlanDetailsId = Guid.Parse("d2c0e2d1-3e5e-4e4c-af4f-c6ce7e6e0003"), PlanId = monthlyId, Value = "Download for offline" },
            new { PlanDetailsId = Guid.Parse("e3d1f3e2-4f6f-5f5d-b05a-d7df8e7e0001"), PlanId = seasonId,  Value = "4K Ultra HD" },
            new { PlanDetailsId = Guid.Parse("e3d1f3e2-4f6f-5f5d-b05a-d7df8e7e0002"), PlanId = seasonId,  Value = "Priority support" },
            new { PlanDetailsId = Guid.Parse("e3d1f3e2-4f6f-5f5d-b05a-d7df8e7e0003"), PlanId = seasonId,  Value = "Exclusive content" },
            new { PlanDetailsId = Guid.Parse("e3d1f3e2-4f6f-5f5d-b05a-d7df8e7e0004"), PlanId = seasonId,  Value = "Early releases" }
        );
    }
}
