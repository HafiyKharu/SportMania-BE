using Discord;
using Discord.WebSocket;
using SportMania.Models;
using SportMania.Repository.Interface;
using SportMania.Services.Interface;

namespace SportMania.Services.Extension
{
    public class DiscordCommandExecution 
        (DiscordSocketClient _client, 
        IServiceProvider _serviceProvider, 
        ILogger<DiscordCommandExecution> _logger,
        IConfiguration _configuration)
    {
        public async Task HandleSetupRolesAsync(SocketSlashCommand command)
        {
            using var scope = _serviceProvider.CreateScope();
            var planRepo = scope.ServiceProvider.GetRequiredService<IPlanRepository>();
            var mappingRepo = scope.ServiceProvider.GetRequiredService<IPlanRoleMappingRepository>();

            if (!await HasAdminPermissionAsync(command))
            {
                await command.RespondAsync("You don't have permission to use this command.", ephemeral: true);
                return;
            }

            var planName = command.Data.Options.First(o => o.Name == "plan").Value.ToString()!;
            var role = (SocketRole)command.Data.Options.First(o => o.Name == "role").Value;

            var plans = await planRepo.GetAllAsync();
            var plan = plans.FirstOrDefault(p => p.Name.Equals(planName, StringComparison.OrdinalIgnoreCase));

            if (plan == null)
            {
                var availablePlans = string.Join(", ", plans.Select(p => $"`{p.Name}`"));
                await command.RespondAsync($"Plan '{planName}' not found.\nAvailable plans: {availablePlans}", ephemeral: true);
                return;
            }

            var existingMapping = await mappingRepo.GetByGuildAndPlanAsync(command.GuildId!.Value, plan.PlanId);
            if (existingMapping != null)
            {
                existingMapping.RoleId = role.Id;
                await mappingRepo.UpdateAsync(existingMapping);
            }
            else
            {
                var mapping = new PlanRoleMapping
                {
                    GuildId = command.GuildId!.Value,
                    PlanId = plan.PlanId,
                    RoleId = role.Id
                };
                await mappingRepo.CreateAsync(mapping);
            }

            await command.RespondAsync($"✅ Mapped **{plan.Name}** → {role.Mention}", ephemeral: true);
        }

        public async Task HandleViewMappingsAsync(SocketSlashCommand command)
        {
            using var scope = _serviceProvider.CreateScope();
            var mappingRepo = scope.ServiceProvider.GetRequiredService<IPlanRoleMappingRepository>();

            if (!await HasAdminPermissionAsync(command))
            {
                await command.RespondAsync("You don't have permission to use this command.", ephemeral: true);
                return;
            }

            var mappings = await mappingRepo.GetByGuildIdAsync(command.GuildId!.Value);
            var mappingList = mappings.ToList();

            if (!mappingList.Any())
            {
                await command.RespondAsync("No plan-to-role mappings configured.\nUse `/setuproles` to configure.", ephemeral: true);
                return;
            }

            var guild = _client.GetGuild(command.GuildId!.Value);
            var description = string.Join("\n", mappingList.Select(m =>
            {
                var role = guild.GetRole(m.RoleId);
                return $"**{m.Plan?.Name}** → {role?.Mention ?? "Unknown Role"}";
            }));

            var embed = new EmbedBuilder()
                .WithTitle("🎭 Plan-to-Role Mappings")
                .WithColor(Color.Blue)
                .WithDescription(description)
                .WithFooter($"Total: {mappingList.Count} mappings")
                .Build();

            await command.RespondAsync(embed: embed, ephemeral: true);
        }

        public async Task HandleRemoveMappingAsync(SocketSlashCommand command)
        {
            using var scope = _serviceProvider.CreateScope();
            var planRepo = scope.ServiceProvider.GetRequiredService<IPlanRepository>();
            var mappingRepo = scope.ServiceProvider.GetRequiredService<IPlanRoleMappingRepository>();

            if (!await HasAdminPermissionAsync(command))
            {
                await command.RespondAsync("You don't have permission to use this command.", ephemeral: true);
                return;
            }

            var planName = command.Data.Options.First(o => o.Name == "plan").Value.ToString()!;
            var plans = await planRepo.GetAllAsync();
            var plan = plans.FirstOrDefault(p => p.Name.Equals(planName, StringComparison.OrdinalIgnoreCase));

            if (plan == null)
            {
                await command.RespondAsync($"Plan '{planName}' not found.", ephemeral: true);
                return;
            }

            await mappingRepo.DeleteByGuildAndPlanAsync(command.GuildId!.Value, plan.PlanId);
            await command.RespondAsync($"✅ Removed mapping for **{plan.Name}**", ephemeral: true);
        }

        public async Task HandleGenerateAsync(SocketSlashCommand command)
        {
            if (!int.TryParse(_configuration["DefaultKeyDurationDays"], out int DefaultKeyDurationDays)) _logger.LogWarning("Config the default Key Duration Days in appsetting."); 
            using var scope = _serviceProvider.CreateScope();
            var keyService = scope.ServiceProvider.GetRequiredService<IKeyService>();
            var planRepo = scope.ServiceProvider.GetRequiredService<IPlanRepository>();

            if (!await HasAdminPermissionAsync(command))
            {
                await command.RespondAsync("You don't have permission to use this command.", ephemeral: true);
                return;
            }

            var planName = command.Data.Options.First(o => o.Name == "plan").Value.ToString()!;

            var plans = await planRepo.GetAllAsync();
            var plan = plans.FirstOrDefault(p => p.Name.Equals(planName, StringComparison.OrdinalIgnoreCase));

            if (plan == null)
            {
                var availablePlans = string.Join(", ", plans.Select(p => $"`{p.Name}`"));
                await command.RespondAsync($"Plan '{planName}' not found.\nAvailable plans: {availablePlans}", ephemeral: true);
                return;
            }

            if (!int.TryParse(plan.Duration, out int durationDays))
            {
                _logger.LogWarning("Could not parse duration '{Duration}' for plan '{PlanName}'. Using default of {DefaultDuration} days.",
                    plan.Duration, plan.Name, DefaultKeyDurationDays);
                durationDays = DefaultKeyDurationDays;
            }

            var generatedKey = await keyService.GenerateKeyAsync(command.GuildId!.Value, plan.PlanId, durationDays);

            var embed = new EmbedBuilder()
                .WithTitle($"🔑 {plan.Name} Key Generated")
                .WithColor(Color.Green)
                .WithDescription($"Generated key for **{plan.Name}** ({durationDays} days)")
                .AddField("Key", $"`{generatedKey.LicenseKey}`")
                .WithTimestamp(DateTimeOffset.Now)
                .Build();

            await command.RespondAsync(embed: embed, ephemeral: true);
            await LogToChannelAsync($"🔑 {command.User} generated {plan.Name} key");
        }

        public async Task HandleRedeemAsync(SocketSlashCommand command)
        {
            if (command.GuildId == null)
            {
                await command.RespondAsync("This command can only be used in a server.", ephemeral: true);
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var keyRepo = scope.ServiceProvider.GetRequiredService<IKeyRepository>();
            var mappingRepo = scope.ServiceProvider.GetRequiredService<IPlanRoleMappingRepository>();

            var licenseKey = command.Data.Options.First(o => o.Name == "key").Value.ToString()!;
            var guildId = command.GuildId.Value;

            var existingKey = await keyRepo.GetByUserIdAndGuildAsync(command.User.Id, guildId);
            if (existingKey != null && existingKey.IsActive)
            {
                await command.RespondAsync("You already have an active license!", ephemeral: true);
                return;
            }

            var key = await keyRepo.GetByLicenseKeyAndGuildAsync(licenseKey, guildId);
            if (key == null)
            {
                await command.RespondAsync("Invalid license key.", ephemeral: true);
                return;
            }

            if (!key.IsActive)
            {
                await command.RespondAsync("This license key is not active.", ephemeral: true);
                return;
            }

            if (key.RedeemedByUserId != null)
            {
                await command.RespondAsync("This key has already been redeemed.", ephemeral: true);
                return;
            }

            key.RedeemedByUserId = command.User.Id;
            key.RedeemedAt = DateTime.UtcNow;
            key.ExpiresAt = DateTime.UtcNow.AddDays(key.DurationDays);
            await keyRepo.UpdateAsync(key);

            var mapping = await mappingRepo.GetByGuildAndPlanAsync(guildId, key.PlanId);
            if (mapping != null)
            {
                var socketGuild = _client.GetGuild(guildId);
                var user = socketGuild.GetUser(command.User.Id);
                var role = socketGuild.GetRole(mapping.RoleId);

                if (role != null && user != null)
                {
                    try
                    {
                        await user.AddRoleAsync(role);
                        _logger.LogInformation("✅ Role '{RoleName}' assigned to user {UserId}", role.Name, command.User.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Failed to assign role '{RoleName}' to user {UserId}", role.Name, command.User.Id);
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ Could not assign role: Role={RoleFound}, User={UserFound}", role != null, user != null);
                }
            }
            else
            {
                _logger.LogWarning("⚠️ No plan-to-role mapping found for PlanId={PlanId} in GuildId={GuildId}. Use /setuproles to configure.", key.PlanId, guildId);
            }

            var embed = new EmbedBuilder()
                .WithTitle("✅ License Activated!")
                .WithColor(Color.Green)
                .AddField("Plan", key.Plan?.Name ?? "Unknown", inline: true)
                .AddField("Duration", $"{key.DurationDays} days", inline: true)
                .AddField("Expires", key.ExpiresAt?.ToString("yyyy-MM-dd HH:mm UTC") ?? "Never")
                .WithFooter("Thank you for your purchase!")
                .WithTimestamp(DateTimeOffset.Now)
                .Build();

            await command.RespondAsync(embed: embed, ephemeral: true);
            await LogToChannelAsync( $"✅ {command.User.Mention} activated {key.Plan?.Name} (expires: {key.ExpiresAt:yyyy-MM-dd})");
        }

        private Task<bool> HasAdminPermissionAsync(SocketSlashCommand command)
        {
            if (command.GuildId == null) return Task.FromResult(false);

            var guild = _client.GetGuild(command.GuildId.Value);
            var user = guild.GetUser(command.User.Id);

            return Task.FromResult(user.GuildPermissions.Administrator || user.GuildPermissions.ManageGuild);
        }

        private async Task LogToChannelAsync(string message)
        {
            var guildIdString = _configuration["Discord:DefaultGuildId"] ?? throw new Exception("Default Guild ID not configured. Please set 'Discord:DefaultGuildId' in appsetting.");
            if (_client.GetChannel(ulong.Parse(guildIdString)) is ITextChannel channel)
            {
                await channel.SendMessageAsync(message);
            }
        }
    }
}