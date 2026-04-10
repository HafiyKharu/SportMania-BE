using Microsoft.EntityFrameworkCore;
using SportMania.Data;
using SportMania.Models;
using SportMania.Repository.Interface;

namespace SportMania.Repository;

public class PlanRoleMappingRepository (ApplicationDbContext _context) : IPlanRoleMappingRepository
{    public async Task<PlanRoleMapping?> GetByGuildAndPlanAsync(ulong guildId, Guid planId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PlanRoleMapping>()
            .Include(m => m.Plan)
            .FirstOrDefaultAsync(m => m.GuildId == guildId && m.PlanId == planId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<PlanRoleMapping>> GetByGuildIdAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PlanRoleMapping>()
            .Include(m => m.Plan)
            .Where(m => m.GuildId == guildId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<PlanRoleMapping> CreateAsync(PlanRoleMapping mapping, CancellationToken cancellationToken = default)
    {
        await _context.Set<PlanRoleMapping>().AddAsync(mapping, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return mapping;
    }

    public async Task UpdateAsync(PlanRoleMapping mapping, CancellationToken cancellationToken = default)
    {
        _context.Set<PlanRoleMapping>().Update(mapping);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid mappingId, CancellationToken cancellationToken = default)
    {
        var mapping = await _context.Set<PlanRoleMapping>().FindAsync(new object[] { mappingId }, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (mapping != null)
        {
            _context.Set<PlanRoleMapping>().Remove(mapping);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task DeleteByGuildAndPlanAsync(ulong guildId, Guid planId, CancellationToken cancellationToken = default)
    {
        var mapping = await GetByGuildAndPlanAsync(guildId, planId, cancellationToken).ConfigureAwait(false);
        if (mapping != null)
        {
            _context.Set<PlanRoleMapping>().Remove(mapping);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}