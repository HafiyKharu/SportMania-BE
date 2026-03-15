using SportMania.Models;

namespace SportMania.Services.Interface;

public interface IKeyService
{
    Task<Key> GenerateKeyAsync(ulong guildId, Guid planId, int durationDays);
}