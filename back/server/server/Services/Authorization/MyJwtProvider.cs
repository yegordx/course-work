using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using server.Interfaces;
using server.Models;
using server.Services.Authorization;

public class MyJwtProvider : IJwtProvider
{
    private readonly JwtOptions _options;

    public MyJwtProvider(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateUserToken(Client client)
    {
        // ✅ Стандартний claim для ID
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, client.Id)
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_options.ExpiersHours),
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
