using Microsoft.EntityFrameworkCore;
using SportMania.Data;
using SportMania.Models;
using SportMania.Repository.Interface;

namespace SportMania.Repository;

public class KeyRepository (ApplicationDbContext _context) : IKeyRepository
{
    public async Task<Key> CreateAsync(Key key, CancellationToken cancellationToken = default)
    {
        await _context.Keys.AddAsync(key, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return key;
    }

    public async Task<Key?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Keys.FirstOrDefaultAsync(k => k.KeyId == id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Key?> GetByLicenseKeyAsync(string licenseKey, CancellationToken cancellationToken = default)
    {
        return await _context.Keys.FirstOrDefaultAsync(k => k.LicenseKey == licenseKey, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Key?> GetByLicenseKeyAndGuildAsync(string licenseKey, ulong guildId, CancellationToken cancellationToken = default)
    {
        return await _context.Keys
            .Include(k => k.Plan)
            .FirstOrDefaultAsync(k => k.LicenseKey == licenseKey && k.GuildId == guildId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Key>> GetByGuildIdAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        return await _context.Keys.Where(k => k.GuildId == guildId).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Key>> GetActiveKeysByGuildIdAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        return await _context.Keys
            .Where(k => k.GuildId == guildId && k.IsActive && k.RedeemedByUserId == null)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Key?> GetByUserIdAndGuildAsync(ulong userId, ulong guildId, CancellationToken cancellationToken = default)
    {
        return await _context.Keys
            .FirstOrDefaultAsync(k => k.RedeemedByUserId == userId && k.GuildId == guildId && k.IsActive, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UpdateAsync(Key key, CancellationToken cancellationToken = default)
    {
        _context.Keys.Update(key);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var key = await GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (key != null)
        {
            _context.Keys.Remove(key);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task DeleteExpiredKeysAsync(CancellationToken cancellationToken = default)
    {
        var expiredKeys = await _context.Keys
            .Where(k => k.ExpiresAt != null && k.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        
        _context.Keys.RemoveRange(expiredKeys);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}