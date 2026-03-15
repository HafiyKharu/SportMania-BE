using SportMania.Models;
using SportMania.Repository.Interface;
using SportMania.Services.Interface;
using System.Security.Cryptography;

namespace SportMania.Services;

public class KeyService(
        IKeyRepository _keyRepository,
        ILogger<KeyService> _logger) : IKeyService
{
    public async Task<Key> GenerateKeyAsync(ulong guildId, Guid planId, int durationDays)
    {
        string licenseKey;
        int maxRetries = 10;
        int retryCount = 0;
        do
        {
            licenseKey = GenerateLicenseKey();
            retryCount++;
            if (retryCount > maxRetries)
            {
                _logger.LogError("Failed to generate a unique license key after {MaxRetries} attempts.", maxRetries);
                throw new InvalidOperationException("Could not generate a unique license key.");
            }
        } while (await _keyRepository.GetByLicenseKeyAsync(licenseKey) != null);

        var newKey = new Key
        {
            KeyId = Guid.NewGuid(),
            LicenseKey = licenseKey,
            GuildId = guildId,
            PlanId = planId,
            DurationDays = durationDays,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return await _keyRepository.CreateAsync(newKey);
    }

    private static string GenerateLicenseKey()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var segments = new string[4];

        for (int i = 0; i < 4; i++)
        {
            var segment = new char[5];
            for (int j = 0; j < 5; j++)
            {
                segment[j] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            }
            segments[i] = new string(segment);
        }

        return string.Join("-", segments);
    }
}