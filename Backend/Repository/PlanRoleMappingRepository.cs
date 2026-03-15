using Microsoft.EntityFrameworkCore;
using SportMania.Data;
using SportMania.Models;
using SportMania.Repository.Interface;

namespace SportMania.Repository;

public class PlanRoleMappingRepository (ApplicationDbContext _context) : IPlanRoleMappingRepository
{    public async Task<PlanRoleMapping?> GetByGuildAndPlanAsync(ulong guildId, Guid planId)
    {
        return await _context.Set<PlanRoleMapping>()
            .Include(m => m.Plan)
            .FirstOrDefaultAsync(m => m.GuildId == guildId && m.PlanId == planId);
    }

    public async Task<IEnumerable<PlanRoleMapping>> GetByGuildIdAsync(ulong guildId)
    {
        return await _context.Set<PlanRoleMapping>()
            .Include(m => m.Plan)
            .Where(m => m.GuildId == guildId)
            .ToListAsync();
    }

    public async Task<PlanRoleMapping> CreateAsync(PlanRoleMapping mapping)
    {
        await _context.Set<PlanRoleMapping>().AddAsync(mapping);
        await _context.SaveChangesAsync();
        return mapping;
    }

    public async Task UpdateAsync(PlanRoleMapping mapping)
    {
        _context.Set<PlanRoleMapping>().Update(mapping);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid mappingId)
    {
        var mapping = await _context.Set<PlanRoleMapping>().FindAsync(mappingId);
        if (mapping != null)
        {
            _context.Set<PlanRoleMapping>().Remove(mapping);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteByGuildAndPlanAsync(ulong guildId, Guid planId)
    {
        var mapping = await GetByGuildAndPlanAsync(guildId, planId);
        if (mapping != null)
        {
            _context.Set<PlanRoleMapping>().Remove(mapping);
            await _context.SaveChangesAsync();
        }
    }
}