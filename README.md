# SportMania

SportMania is a subscription + payment + Discord role access system.

- Backend: ASP.NET Core Web API + EF Core (PostgreSQL) + ToyyibPay integration + optional Discord bot
- Frontend: Blazor (Interactive Server) UI for Plans, Transactions, and payment result pages

## Latest Updates

- Payment flow fixed to use strongly-typed DTOs (no JsonElement runtime errors)
- ToyyibPay callback now redirects to Frontend success/failed pages (instead of stopping on Backend API route)
- Key endpoints + service added to retrieve license keys by transaction
- Plan image upload is Frontend-only:
    - files are saved under Frontend/wwwroot/Media
    - the database stores a relative path like Media/your-image.png
- Development quality-of-life:
    - dotnet watch can reload when Media files change; the Frontend project excludes wwwroot/Media from watch

## Repository Layout

- Backend/: Web API, DB access, ToyyibPay callback handling, Discord bot services
- Frontend/: Blazor UI, file uploads to wwwroot/Media, API services calling Backend

## Prerequisites

- .NET 10 SDK
- PostgreSQL database
- ToyyibPay account (sandbox or production)
- Discord bot token (optional; only if you enable the bot)

## Configuration

### Backend configuration

Update Backend/appsettings.json or (recommended) use dotnet user-secrets.

Minimal example (do not commit real secrets):

```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Host=localhost;Database=sportmania;Username=postgres;Password=your_password"
    },
    "Frontend": {
        "BaseUrl": "http://localhost:3000"
    },
    "Backend": {
        "PublicBaseUrl": "http://localhost:5235"
    },
    "Cors": {
        "AllowedOrigins": ["http://localhost:3000"]
    },
    "ToyyibPay": {
        "UserSecretKey": "YOUR_TOYYIBPAY_SECRET",
        "CategoryCodes": {
            "Seasonal": "YOUR_CATEGORY_CODE",
            "Daily": "YOUR_CATEGORY_CODE",
            "Monthly": "YOUR_CATEGORY_CODE",
            "Weekly": "YOUR_CATEGORY_CODE"
        }
    },
    "Discord": {
        "BotToken": "YOUR_DISCORD_BOT_TOKEN"
    },
    "DiscordBot": {
        "Enabled": true
    },
    "DefaultGuildId": 12341234
}
```

### Frontend API base URL

Frontend reads ApiBaseUrl (defaults to http://localhost:5235).

### Redirect configuration for payment callback

Backend now supports environment-based redirect settings for production deployments:

- `Frontend:BaseUrl`: where users are redirected after callback (success/failed pages)
- `Backend:PublicBaseUrl`: public backend URL used when creating ToyyibPay callback URL
- `Cors:AllowedOrigins`: allowed frontend origins for API calls

Example production values:

```json
{
    "Frontend": {
        "BaseUrl": "https://app.sportmania.com"
    },
    "Backend": {
        "PublicBaseUrl": "https://api.sportmania.com"
    },
    "Cors": {
        "AllowedOrigins": ["https://app.sportmania.com"]
    }
}
```

## Run (Development)

### 1) Start Backend

```bash
dotnet run --project Backend/SportMania.csproj
```

### 2) Start Frontend

```bash
dotnet run --project Frontend/BlazorApp.csproj
```

## dotnet watch notes (important for image upload)

When you upload an image, the app writes a file into Frontend/wwwroot/Media.
dotnet watch treats that as a file change and triggers Browser Refresh / Hot Reload, which looks like a full page reload.

Recommended options:

- Run without hot reload:

```bash
dotnet watch run --project Frontend/BlazorApp.csproj --no-hot-reload
```

- Or keep watch but ignore the upload folder (already configured in Frontend/BlazorApp.csproj):

```xml
<ItemGroup>
    <Watch Remove="wwwroot/Media/**" />
</ItemGroup>
```

## Plan image rules

- Upload destination: Frontend/wwwroot/Media/
- Value saved to database: Media/<filename.ext>
- UI preview: <img src="@plan.ImageUrl" ... />

## Payment flow (high level)

1. Frontend calls Backend to initiate payment
2. Backend creates a ToyyibPay bill and returns redirectUrl
3. User pays on ToyyibPay
4. ToyyibPay calls Backend callback endpoint
5. Backend updates the transaction and redirects user to Frontend:
     - /transactions/success/{transactionId}
     - /transactions/failed/{transactionId}

## Discord Commands

The following slash commands are available for managing the bot.

---

### /setuproles

Maps a subscription plan to a Discord role. Users who purchase or redeem a key for this plan will be granted the specified role.

- Syntax: /setuproles plan:<Plan Name> role:<@Role>
- Permissions: Administrator only

Examples:
- /setuproles plan:Season Pass role:@Season Pass Holder
- /setuproles plan:Monthly Plan role:@Subscriber
- /setuproles plan:Weekly Pass role:@Weekly User

---

### /viewmappings

Displays all current plan-to-role mappings for the server.

- Syntax: /viewmappings
- Permissions: Administrator only

---

### /removemapping

Removes an existing plan-to-role mapping.

- Syntax: /removemapping plan:<Plan Name>
- Permissions: Administrator only

Example:
- /removemapping plan:Weekly Pass

---

### /generate

Generates one or more license keys for a specific plan.

- Syntax: /generate plan:<Plan Name> amount:[Number]
- Permissions: Administrator only

Examples:
- /generate plan:Monthly Plan amount:10 (Generates 10 keys)
- /generate plan:Season Pass (Generates 1 key)

---

### /redeem

Allows a user to redeem a license key to gain access and the associated role.

- Syntax: /redeem key:<License Key>
- Permissions: All users

Example:
- /redeem key:ABCD-EFGH-IJKL-MNOP-QRST#
