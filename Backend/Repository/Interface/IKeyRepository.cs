using SportMania.Models;
using System;
using System.Threading.Tasks;

namespace SportMania.Repository.Interface
{
    public interface IKeyRepository
    {
        Task<Key> CreateAsync(Key key, CancellationToken cancellationToken = default);
        Task<Key?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Key?> GetByLicenseKeyAsync(string licenseKey, CancellationToken cancellationToken = default);
        Task<Key?> GetByLicenseKeyAndGuildAsync(string licenseKey, ulong guildId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Key>> GetByGuildIdAsync(ulong guildId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Key>> GetActiveKeysByGuildIdAsync(ulong guildId, CancellationToken cancellationToken = default);
        Task<Key?> GetByUserIdAndGuildAsync(ulong userId, ulong guildId, CancellationToken cancellationToken = default);
        Task UpdateAsync(Key key, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task DeleteExpiredKeysAsync(CancellationToken cancellationToken = default);
    }
}