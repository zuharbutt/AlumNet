using AlumniManagementSystem.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AlumniManagementSystem.Services;

public class JwtService(IConfiguration configuration)
{
    public (string token, DateTime expiresAt) GenerateToken(User user)
    {
        var key     = configuration["Jwt:Key"]      ?? throw new InvalidOperationException("Jwt:Key missing");
        var issuer  = configuration["Jwt:Issuer"]   ?? "AlumniMS";
        var audience= configuration["Jwt:Audience"] ?? "AlumniMS";
        var hours   = int.TryParse(configuration["Jwt:ExpiresInHours"], out var h) ? h : 24;

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds      = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expiresAt  = DateTime.UtcNow.AddHours(hours);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email,          user.Email),
            new Claim(ClaimTypes.Name,           user.FullName),
            new Claim(ClaimTypes.Role,           user.Role),
            new Claim("userId",                  user.UserId.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            expires:            expiresAt,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
