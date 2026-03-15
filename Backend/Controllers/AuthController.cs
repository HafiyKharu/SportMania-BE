using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SportMania.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("login/{username}+{password}")]
    public IActionResult Login(string username, string password)
    {
        var authSection = _configuration.GetSection("Auth");
        var expectedPassword = authSection["LoginPassword"];

        if (string.IsNullOrWhiteSpace(expectedPassword))
        {
            return StatusCode(500, "Login password is not configured.");
        }

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return BadRequest("Username and password are required.");
        }

        if (!string.Equals(password, expectedPassword, StringComparison.Ordinal))
        {
            return Unauthorized("Invalid credentials.");
        }

        var adminEmails = authSection.GetSection("AdminEmails").Get<string[]>() ?? Array.Empty<string>();
        var normalizedEmail = username.Trim();
        var role = adminEmails.Any(email =>
            string.Equals(email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
            ? "Admin"
            : "User";

        var tokenMinutes = authSection.GetValue<int>("TokenMinutes", 60);
        var expiresAt = DateTime.UtcNow.AddMinutes(tokenMinutes);
        var token = GenerateJwtToken(normalizedEmail, role, expiresAt);

        return Ok(new
        {
            token,
            role,
            email = normalizedEmail,
            expiresAt
        });
    }

    private string GenerateJwtToken(string email, string role, DateTime expiresAt)
    {
        var authSection = _configuration.GetSection("Auth");
        var issuer = authSection["Issuer"] ?? "SportMania";
        var audience = authSection["Audience"] ?? "SportManiaFrontend";
        var key = authSection["JwtKey"] ?? throw new InvalidOperationException("Auth:JwtKey is not configured.");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, email),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Role, role)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
