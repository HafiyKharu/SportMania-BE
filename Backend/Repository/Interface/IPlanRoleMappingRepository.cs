using SportMania.Models;

namespace SportMania.Repository.Interface;

public interface IPlanRoleMappingRepository
{
    Task<PlanRoleMapping?> GetByGuildAndPlanAsync(ulong guildId, Guid planId);
    Task<IEnumerable<PlanRoleMapping>> GetByGuildIdAsync(ulong guildId);
    Task<PlanRoleMapping> CreateAsync(PlanRoleMapping mapping);
    Task UpdateAsync(PlanRoleMapping mapping);
    Task DeleteAsync(Guid mappingId);
    Task DeleteByGuildAndPlanAsync(ulong guildId, Guid planId);
}