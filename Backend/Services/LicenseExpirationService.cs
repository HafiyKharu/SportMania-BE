using SportMania.Repository.Interface;

namespace SportMania.Services;

public class LicenseExpirationService(
        IServiceProvider _serviceProvider,
        ILogger<LicenseExpirationService> _logger,
        IConfiguration _configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!int.TryParse(_configuration["CheckInterval"], out int checkIntervalHours))
        {
            _logger.LogError("Check Interval must be provided and cannot be empty, Check Appsetting.");
            return;
        }

        var checkInterval = TimeSpan.FromHours(checkIntervalHours);
        _logger.LogInformation($"License expiration check will run every {checkIntervalHours} hour(s)");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckExpiredLicenses(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking expired licenses");
            }

            try
            {
                await Task.Delay(checkInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown, no need to log
                break;
            }
        }
    }

    private async Task CheckExpiredLicenses(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var keyRepo = scope.ServiceProvider.GetRequiredService<IKeyRepository>();

        await keyRepo.DeleteExpiredKeysAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Expired licenses cleanup completed");
    }
}