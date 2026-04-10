using SportMania.Models;

namespace SportMania.Repository.Interface;

public interface IPlanRoleMappingRepository
{
    Task<PlanRoleMapping?> GetByGuildAndPlanAsync(ulong guildId, Guid planId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PlanRoleMapping>> GetByGuildIdAsync(ulong guildId, CancellationToken cancellationToken = default);
    Task<PlanRoleMapping> CreateAsync(PlanRoleMapping mapping, CancellationToken cancellationToken = default);
    Task UpdateAsync(PlanRoleMapping mapping, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid mappingId, CancellationToken cancellationToken = default);
    Task DeleteByGuildAndPlanAsync(ulong guildId, Guid planId, CancellationToken cancellationToken = default);
}