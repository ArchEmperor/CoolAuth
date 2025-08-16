using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CoolAuth.Data;
using CoolAuth.Data.Entities;
using CoolAuth.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace CoolAuth.Services;

public sealed class JwtService(IConfiguration config)
{
    /*#if DEBUG
    private const int AccessTokenLifetime = 200; //minutes
    #else*/
    private const int AccessTokenLifetime = 10; //minutes
    /*#endif*/

    private const string UserIdClaimType = "userid";
    public string GenerateAccessToken(List<Claim> claims)
    {
        var lifeTime = TimeSpan.FromMinutes(AccessTokenLifetime);
        
        var key = Encoding.UTF8.GetBytes(config["JwtSettings:Key"]!);

        var token = new JwtSecurityToken(
            claims: claims,
            issuer: config["JwtSettings:Issuer"],
            audience: config["JwtSettings:Audience"],
            expires: DateTime.UtcNow.Add(lifeTime),
            notBefore: DateTime.UtcNow,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public static JwtSecurityToken? ReadToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        return handler.CanReadToken(token) ? handler.ReadJwtToken(token) : null;
    }
    public static bool GetUserIdFromHttpContext(HttpContext context, out int userId)
    {
        userId = 0;
        var userIdClaim = context.User.Claims.FirstOrDefault(o => o.Type == UserIdClaimType);
        return userIdClaim != null && int.TryParse(userIdClaim.Value, out userId);
    }
}