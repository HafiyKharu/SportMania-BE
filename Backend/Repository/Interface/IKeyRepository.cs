using SportMania.Models;
using System;
using System.Threading.Tasks;

namespace SportMania.Repository.Interface
{
    public interface IKeyRepository
    {
        Task<Key> CreateAsync(Key key);
        Task<Key?> GetByIdAsync(Guid id);
        Task<Key?> GetByLicenseKeyAsync(string licenseKey);
        Task<Key?> GetByLicenseKeyAndGuildAsync(string licenseKey, ulong guildId);
        Task<IEnumerable<Key>> GetByGuildIdAsync(ulong guildId);
        Task<IEnumerable<Key>> GetActiveKeysByGuildIdAsync(ulong guildId);
        Task<Key?> GetByUserIdAndGuildAsync(ulong userId, ulong guildId);
        Task UpdateAsync(Key key);
        Task DeleteAsync(Guid id);
        Task DeleteExpiredKeysAsync();
    }
}