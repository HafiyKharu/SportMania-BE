using Microsoft.EntityFrameworkCore;
using SportMania.Data;
using SportMania.Models;
using SportMania.Repository.Interface;

namespace SportMania.Repository;

public class KeyRepository (ApplicationDbContext _context) : IKeyRepository
{
    public async Task<Key> CreateAsync(Key key)
    {
        await _context.Keys.AddAsync(key);
        await _context.SaveChangesAsync();
        return key;
    }

    public async Task<Key?> GetByIdAsync(Guid id)
    {
        return await _context.Keys.FirstOrDefaultAsync(k => k.KeyId == id);
    }

    public async Task<Key?> GetByLicenseKeyAsync(string licenseKey)
    {
        return await _context.Keys.FirstOrDefaultAsync(k => k.LicenseKey == licenseKey);
    }

    public async Task<Key?> GetByLicenseKeyAndGuildAsync(string licenseKey, ulong guildId)
    {
        return await _context.Keys
            .Include(k => k.Plan)
            .FirstOrDefaultAsync(k => k.LicenseKey == licenseKey && k.GuildId == guildId);
    }

    public async Task<IEnumerable<Key>> GetByGuildIdAsync(ulong guildId)
    {
        return await _context.Keys.Where(k => k.GuildId == guildId).ToListAsync();
    }

    public async Task<IEnumerable<Key>> GetActiveKeysByGuildIdAsync(ulong guildId)
    {
        return await _context.Keys
            .Where(k => k.GuildId == guildId && k.IsActive && k.RedeemedByUserId == null)
            .ToListAsync();
    }

    public async Task<Key?> GetByUserIdAndGuildAsync(ulong userId, ulong guildId)
    {
        return await _context.Keys
            .FirstOrDefaultAsync(k => k.RedeemedByUserId == userId && k.GuildId == guildId && k.IsActive);
    }

    public async Task UpdateAsync(Key key)
    {
        _context.Keys.Update(key);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var key = await GetByIdAsync(id);
        if (key != null)
        {
            _context.Keys.Remove(key);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteExpiredKeysAsync()
    {
        var expiredKeys = await _context.Keys
            .Where(k => k.ExpiresAt != null && k.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();
        
        _context.Keys.RemoveRange(expiredKeys);
        await _context.SaveChangesAsync();
    }
}