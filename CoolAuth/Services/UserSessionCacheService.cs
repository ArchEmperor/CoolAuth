using System.Text.Json;
using AutoMapper;
using CoolAuth.Data.Entities;
using CoolAuth.DTO;

namespace CoolAuth.Services;

public class UserSessionCacheService(IMapper mapper,ICacheService cache)
{
    public async Task CacheSessionAsync(Session session,DateTimeOffset expires)
    {
        var dto = mapper.Map<SessionCacheDto>(session);

        var sessionStr = JsonSerializer.Serialize(dto);
        var refreshTokenKey = $"refresh_token:{session.RefreshToken}";
        var cacheLifetime = expires - DateTimeOffset.UtcNow;

        await cache.SetAsync(refreshTokenKey, sessionStr, cacheLifetime);

        var aliasKey = $"session:{session.SessionId}";
        await cache.SetAsync(aliasKey, refreshTokenKey, cacheLifetime);
    }
    public async Task DeleteCachedSessionAsync(Guid sessionId)
    {
        var sessionKey = $"session:{sessionId}";
        var refreshTokenKey = await cache.GetAsync(sessionKey);
        if (refreshTokenKey != null)
        {
            await cache.RemoveAsync(refreshTokenKey);
        }
        await cache.RemoveAsync(sessionKey);
    }
    public async Task<Session?> GetCachedSessionByIdAsync(Guid sessionId)
    {
        var aliasKey = $"session:{sessionId}";
        var refreshTokenStr = await cache.GetAsync(aliasKey);

        if (refreshTokenStr is null) return null;
        var sessionStr = await cache.GetAsync(refreshTokenStr);
        if (sessionStr is null) return null;
        var dto = JsonSerializer.Deserialize<SessionCacheDto>(sessionStr);
        return dto is not null ? mapper.Map<Session>(dto) : null;
    }

    public async Task<Session?> GetCachedSessionByTokenAsync(string token)
    {
        var aliasKey = $"refresh_token:{token}";
        var sessionStr = await cache.GetAsync(aliasKey);

        if (sessionStr is null) return null;
        var dto = JsonSerializer.Deserialize<SessionCacheDto>(sessionStr);
        return dto is not null ? mapper.Map<Session>(dto) : null;
    }
    public async Task CacheUserAsync(User user, TimeSpan? expiry = null)
    {
        var userStr = JsonSerializer.Serialize(user);
        var userKey = $"user:{user.Id}";
        await cache.SetAsync(userKey, userStr, expiry);
        var emailAliasKey = $"user_email:{user.Email}";
        await cache.SetAsync(emailAliasKey, userKey, expiry);
    }

    public async Task<User?> GetCachedUserByIdAsync(int userId)
    {
        var userKey = $"user:{userId}";
        var userStr = await cache.GetAsync(userKey);
        return userStr is null ? null : JsonSerializer.Deserialize<User>(userStr);
    }

    public async Task<User?> GetCachedUserByEmailAsync(string email)
    {
        var emailAliasKey = $"user_email:{email}";
        var userKey = await cache.GetAsync(emailAliasKey);
        
        if (userKey is null) return null;
        var userStr = await cache.GetAsync(userKey);
        return userStr is null ? null : JsonSerializer.Deserialize<User>(userStr);
    }

    public async Task DeleteCachedUserAsync(int userId, string email)
    {
        var userKey = $"user:{userId}";
        var emailAliasKey = $"user_email:{email}";
        
        await cache.RemoveAsync(userKey);
        await cache.RemoveAsync(emailAliasKey);
    }
}