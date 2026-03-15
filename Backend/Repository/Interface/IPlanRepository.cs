using SportMania.Models;

namespace SportMania.Repository.Interface;

public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(Guid id);
    Task<IEnumerable<Plan>> GetAllAsync();
    Task AddAsync(Plan plan);
    Task UpdateAsync(Plan plan);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}