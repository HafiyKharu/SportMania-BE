using Microsoft.EntityFrameworkCore;
using SportMania.Data;
using SportMania.Models;
using SportMania.Repository.Interface;

namespace SportMania.Repository;

public class PlanRepository (ApplicationDbContext _context, IPlanDetailsRepository _detailsRepository) : IPlanRepository
{
    public async Task<Plan?> GetByIdAsync(Guid id)
    {
        return await _context.Plans
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.PlanId == id);
    }

    public async Task<IEnumerable<Plan>> GetAllAsync()
    {
        return await _context.Plans.AsNoTracking()
            .Include(p => p.Details)
            .Where(p => !p.IsDeleted)
            .Where(p => !string.IsNullOrEmpty(p.CategoryCode))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Plan plan)
    {
        plan.PlanId = Guid.NewGuid();
        plan.Details = plan.Details
            .Where(d => !string.IsNullOrWhiteSpace(d.Value))
            .Select(d => new PlanDetails { PlanDetailsId = Guid.NewGuid(), Value = d.Value })
            .ToList();

        await _context.Plans.AddAsync(plan);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Plan plan)
    {
        var existing = await _context.Plans.FindAsync(plan.PlanId);
        if (existing == null) return;

        // 1. Update scalar properties of the Plan
        _context.Entry(existing).CurrentValues.SetValues(plan);

        // 2. Delegate Details management to its own repository
        await _detailsRepository.UpsertForPlanAsync(plan.PlanId, plan.Details);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var plan = await GetByIdAsync(id);
        if (plan != null)
        {
            plan.IsDeleted = true;
            plan.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Plans.AnyAsync(p => p.PlanId == id && !p.IsDeleted);
    }
}