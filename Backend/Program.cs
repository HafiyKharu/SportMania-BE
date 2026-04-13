using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SportMania.Data;
using SportMania.Repository.Interface;
using SportMania.Repository;
using SportMania.Services.Interface;
using SportMania.Services;
using SportMania.Services.Extension;
using Discord;
using Discord.WebSocket;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000"];

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;

    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add database connection.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

var authSection = builder.Configuration.GetSection("Auth");
var jwtKey = authSection["JwtKey"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("Auth:JwtKey is not configured.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = authSection["Issuer"],
            ValidAudience = authSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Add repositories
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<IPlanDetailsRepository, PlanDetailsRepository>();
builder.Services.AddScoped<IKeyRepository, KeyRepository>();
builder.Services.AddScoped<IPlanRoleMappingRepository, PlanRoleMappingRepository>();

// Add services
builder.Services.AddScoped<IKeyService, KeyService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IToyyibPayService, ToyyibPayService>();

// Conditionally register Discord bot services
if (builder.Configuration.GetValue<bool>("DiscordBot:Enabled"))
{
    builder.Services.AddScoped<DiscordCommandExecution>();

    // Register DiscordSocketClient as singleton
    builder.Services.AddSingleton<DiscordSocketClient>(provider =>
    {
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds |
                            GatewayIntents.GuildMessages |
                            GatewayIntents.MessageContent |
                            GatewayIntents.GuildMembers
        };
        return new DiscordSocketClient(config);
    });

    // Register bot service as singleton and as a hosted service to start it
    builder.Services.AddSingleton<IDiscordBotService, DiscordBotService>();
    builder.Services.AddHostedService(provider => (DiscordBotService)provider.GetRequiredService<IDiscordBotService>());
}

// Add hosted service for license expiration
builder.Services.AddHostedService<LicenseExpirationService>();

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Enable CORS
app.UseCors("AllowFrontend");

app.MapControllers();

app.Run();
