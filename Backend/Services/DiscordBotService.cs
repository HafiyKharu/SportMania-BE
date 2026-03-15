using Discord;
using Discord.WebSocket;
using SportMania.Services.Extension;
using SportMania.Services.Interface;

namespace SportMania.Services;

public class DiscordBotService(
        DiscordSocketClient _client,
        IConfiguration _configuration,
        IServiceProvider _serviceProvider,
        ILogger<DiscordBotService> _logger) : IDiscordBotService, IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var token = _configuration["Discord:BotToken"]
            ?? throw new InvalidOperationException("Discord bot token not configured.");

        // Attach event handlers
        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.SlashCommandExecuted += SlashCommandExecutedAsync;

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Detach event handlers
            _client.Log -= LogAsync;
            _client.Ready -= ReadyAsync;
            _client.SlashCommandExecuted -= SlashCommandExecutedAsync;

            await _client.StopAsync();
        }
        catch (ObjectDisposedException)
        {
            // Expected during shutdown if client is already disposed
        }
        catch (OperationCanceledException)
        {
            // Expected if cancellation token is triggered
        }
    }

    private Task LogAsync(LogMessage log)
    {
        _logger.LogInformation("{Message}", log.ToString());
        return Task.CompletedTask;
    }

    private async Task ReadyAsync()
    {
        _logger.LogInformation("🤖 Discord bot is ready. Connected as {User}", _client.CurrentUser);

        var commands = new List<SlashCommandBuilder>
        {
            new SlashCommandBuilder()
                .WithName("setuproles")
                .WithDescription("Set up plan-to-role mapping")
                .AddOption("plan", ApplicationCommandOptionType.String, "Plan name", isRequired: true)
                .AddOption("role", ApplicationCommandOptionType.Role, "Discord role", isRequired: true),

            new SlashCommandBuilder()
                .WithName("viewmappings")
                .WithDescription("View all plan-to-role mappings"),

            new SlashCommandBuilder()
                .WithName("removemapping")
                .WithDescription("Remove a plan-to-role mapping")
                .AddOption("plan", ApplicationCommandOptionType.String, "Plan name", isRequired: true),

            new SlashCommandBuilder()
                .WithName("generate")
                .WithDescription("Generate license keys")
                .AddOption("plan", ApplicationCommandOptionType.String, "Plan name", isRequired: true)
                .AddOption("amount", ApplicationCommandOptionType.Integer, "Number of keys", isRequired: false),

            new SlashCommandBuilder()
                .WithName("redeem")
                .WithDescription("Redeem a license key")
                .AddOption("key", ApplicationCommandOptionType.String, "License key", isRequired: true)
        };

        try
        {
            _logger.LogInformation("⏳ Registering global commands...");

            var builtCommands = commands.Select(c => c.Build()).ToList();
            await _client.BulkOverwriteGlobalApplicationCommandsAsync(builtCommands.ToArray());

            _logger.LogInformation("🎉 Successfully registered {Count} global commands.", builtCommands.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to register global commands.");
        }
    }

    private async Task SlashCommandExecutedAsync(SocketSlashCommand command)
    {
        _logger.LogInformation("⚡ Command received: {CommandName} from {User}", command.CommandName, command.User);

        try
        {
            // Create a scope to get scoped services
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<DiscordCommandExecution>();

            await (command.CommandName switch
            {
                "setuproles" => handler.HandleSetupRolesAsync(command),
                "viewmappings" => handler.HandleViewMappingsAsync(command),
                "removemapping" => handler.HandleRemoveMappingAsync(command),
                "generate" => handler.HandleGenerateAsync(command),
                "redeem" => handler.HandleRedeemAsync(command),
                _ => command.RespondAsync("❌ Unknown command.", ephemeral: true)
            });

            _logger.LogInformation("✅ Command completed: {CommandName}", command.CommandName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error executing command {Command}", command.CommandName);

            if (!command.HasResponded)
            {
                await command.RespondAsync("An error occurred while processing the command.", ephemeral: true);
            }
        }
    }
}