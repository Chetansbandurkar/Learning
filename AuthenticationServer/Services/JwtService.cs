using AuthenticationServer.Configuration;
using AuthenticationServer.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenticationServer.Services;

public class JwtService(IOptions<JwtSettings> jwtOptions)
    : IJwtService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Key));

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Email,
                user.Email),

            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),

            new Claim(
                ClaimTypes.Role,
                user.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),

            Expires = DateTime.UtcNow.AddMinutes(
                _jwtSettings.ExpiryInMinutes),

            Issuer = _jwtSettings.Issuer,

            Audience = _jwtSettings.Audience,

            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}