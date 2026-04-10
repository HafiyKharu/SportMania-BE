using Microsoft.EntityFrameworkCore;
using SportMania.Data;
using SportMania.Models;
using SportMania.Repository.Interface;

namespace SportMania.Repository;

public class PlanRepository (ApplicationDbContext _context, IPlanDetailsRepository _detailsRepository) : IPlanRepository
{
    public async Task<Plan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Plans
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.PlanId == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Plan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Plans.AsNoTracking()
            .Include(p => p.Details)
            .Where(p => !p.IsDeleted)
            .Where(p => !string.IsNullOrEmpty(p.CategoryCode))
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task AddAsync(Plan plan, CancellationToken cancellationToken = default)
    {
        plan.PlanId = Guid.NewGuid();
        plan.Details = plan.Details
            .Where(d => !string.IsNullOrWhiteSpace(d.Value))
            .Select(d => new PlanDetails { PlanDetailsId = Guid.NewGuid(), Value = d.Value })
            .ToList();

        await _context.Plans.AddAsync(plan, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Plan plan, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Plans.FindAsync(new object[] { plan.PlanId }, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (existing == null) return;

        // 1. Update scalar properties of the Plan
        _context.Entry(existing).CurrentValues.SetValues(plan);

        // 2. Delegate Details management to its own repository
        await _detailsRepository.UpsertForPlanAsync(plan.PlanId, plan.Details, cancellationToken).ConfigureAwait(false);

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (plan != null)
        {
            plan.IsDeleted = true;
            plan.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Plans.AnyAsync(p => p.PlanId == id && !p.IsDeleted, cancellationToken).ConfigureAwait(false);
    }
}