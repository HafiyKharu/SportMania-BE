namespace SportMania.Services.Interface;

public interface IDiscordBotService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}