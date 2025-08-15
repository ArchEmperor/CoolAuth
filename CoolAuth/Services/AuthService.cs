using System.Security.Claims;
using System.Text.Json;
using AutoMapper;
using CoolAuth.Data.Entities;
using CoolAuth.Domain;
using CoolAuth.DTO;
using CoolAuth.DTOs;
using CoolAuth.Repositories;
using CoolAuth.Requests;
using CoolAuth.Services.Abstraction;

namespace CoolAuth.Services;

public class AuthService(IUserRepository users, ISessionRepository sessions, JwtService jwtService,ICacheService cache,IMapper mapper) : IAuthService
{
    private const int RefreshTokenExtendedLifetime = 7; //days
    private const int RefreshTokenLifetime = 30;//minutes
    private const int MaxSessionsAmount = 5;
    public async Task<TokensDTO> LoginAsync(LoginRequest request,SessionConnectionInfoDTO info)
    {
        var user =await  users.GetByEmailAsync(request.Email);
        if (user == null||!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            throw DomainException.InvalidCredentials;
        }
        return await CreateNewSession(user,request.Remember,info,request.Fingerprint);
    }

    public async Task<TokensDTO> SignUpAsync(SignUpRequest request,SessionConnectionInfoDTO info)
    {
        if (await users.ExistsByEmailAsync(request.Email))
        {
            throw DomainException.UserAlreadyExists;
        }

        var user= await users.AddAsync(new User()
        {
            Email = request.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Username = request.Username,
            JoinedAt = DateTime.UtcNow,
        });
        return await CreateNewSession(user,false,info,request.Fingerprint);
    }
    public async Task SignOutAsync(string refreshToken)
    {
        Session? session = null;
        session = await GetCachedSessionByToken(refreshToken) ?? 
                  await sessions.GetByRefreshTokenAsync(refreshToken);
        if (session==null)
        {
            throw DomainException.InvalidAuthToken;
        }
        await sessions.DeleteAsync(session.SessionId);
        await DeleteCachedSession(session.SessionId);
    }

    public async Task<TokensDTO> RefreshSessionAsync(RefreshSessionRequest request,SessionConnectionInfoDTO info)
    {
        var accessToken = JwtService.ReadToken(request.AccessToken);
        if (accessToken == null || !Guid.TryParse(accessToken.Payload["session_id"].ToString(), out var sessionId))
        {
            throw DomainException.InvalidAuthToken;
        }

        Session? session = null;
        session = await GetCachedSessionByToken(request.RefreshToken) ?? 
                  await sessions.GetByRefreshTokenAsync(request.RefreshToken);
        if (session==null ||session.SessionId != sessionId)
        {
            throw DomainException.InvalidAuthToken;
        }
        //Expired
        if (session.ExpiresAt <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            await sessions.DeleteAsync(session.SessionId);
            throw DomainException.SessionExpired;
        }
        var user = await users.GetByIdAsync(session.UserId);
        if (!bool.TryParse(accessToken.Payload["remember"].ToString(), out var remember))
        {
            remember = false;
        }
        return await CreateNewSession(user!,remember,info,request.Fingerprint,sessionId);
    }

    public async Task RevokeAsync(HttpContext context,bool revokeAll=false, Guid? sessionId=null)
    {
        if (!revokeAll&&sessionId!=null)
        {
            await sessions.DeleteAsync(sessionId.Value);
            await DeleteCachedSession(sessionId.Value);
            return;
        }

        if (!JwtService.GetUserIdFromHttpContext(context, out var userId))
        {
            throw DomainException.InvalidAuthToken;
        }
        var sessionIds= await sessions.GetAllSessionsIdsAsync(userId);
        await sessions.DeleteAllSessionsAsync(userId);
        foreach (var id in sessionIds)
        {
            await DeleteCachedSession(id);
        }
    }

    public async Task<IReadOnlyList<SessionPartialDto>> GetSessionsAsync(HttpContext context)
    {
        if (!JwtService.GetUserIdFromHttpContext(context, out var userId))
        {
            throw DomainException.InvalidAuthToken;
        }
        return await sessions.GetAllSessionsAsync(userId);
    }
    
    private async Task CacheSession(Session session,DateTimeOffset expires)
    {
        var dto = mapper.Map<SessionCacheDto>(session);

        var sessionStr = JsonSerializer.Serialize(dto);
        var refreshTokenKey = $"refresh_token:{session.RefreshToken}";
        var cacheLifetime = expires - DateTimeOffset.UtcNow;

        await cache.SetAsync(refreshTokenKey, sessionStr, cacheLifetime);

        var aliasKey = $"session:{session.SessionId}";
        await cache.SetAsync(aliasKey, refreshTokenKey, cacheLifetime);
    }
    private async Task DeleteCachedSession(Guid sessionId)
    {
        var sessionKey = $"session:{sessionId}";
        var refreshTokenKey = await cache.GetAsync(sessionKey);
        if (refreshTokenKey != null)
        {
            await cache.RemoveAsync(refreshTokenKey);
        }
        await cache.RemoveAsync(sessionKey);
    }
    private async Task<Session?> GetCachedSessionById(Guid sessionId)
    {
        var aliasKey = $"session:{sessionId}";
        var refreshTokenStr = await cache.GetAsync(aliasKey);

        if (refreshTokenStr is null) return null;
        var sessionStr = await cache.GetAsync(refreshTokenStr);
        if (sessionStr is null) return null;
        var dto = JsonSerializer.Deserialize<SessionCacheDto>(sessionStr);
        return dto is not null ? mapper.Map<Session>(dto) : null;
    }

    private async Task<Session?> GetCachedSessionByToken(string token)
    {
        var aliasKey = $"refresh_token:{token}";
        var sessionStr = await cache.GetAsync(aliasKey);

        if (sessionStr is null) return null;
        var dto = JsonSerializer.Deserialize<SessionCacheDto>(sessionStr);
        return dto is not null ? mapper.Map<Session>(dto) : null;
    }
    private async Task<TokensDTO> CreateNewSession(User user,bool extended,SessionConnectionInfoDTO info, string? fingerprint,Guid? sessionId = null)
    {
        var refreshToken = Guid.NewGuid();
        var expires= extended ? DateTimeOffset.UtcNow.AddDays(RefreshTokenExtendedLifetime) :
            DateTimeOffset.UtcNow.AddMinutes(RefreshTokenLifetime);
        
        var userSessions = await sessions.GetAllSessionsAsync(user.Id);
        if (userSessions.Count() >= MaxSessionsAmount)
        {
            await sessions.DeleteAsync(userSessions.First().SessionId);
        }

        var tempSession = new Session
        {
            ExpiresAt = expires.ToUnixTimeSeconds(),
            UserId = user.Id,
            RefreshToken = refreshToken.ToString(),
            IpAddress = info.IpAddress,
            UserAgent = info.UserAgent,
            Fingerprint = fingerprint,
            Country = info.Country,
            City = info.City,
            Latitude = info.Latitude,
            Longitude = info.Longitude,
            LastRefreshAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };
        if (sessionId != null)
        {
            tempSession.SessionId = sessionId.Value;
        }
        var newSession = await sessions.UpdateAsync(tempSession);
        await CacheSession(newSession, expires);
        
        var accessToken = jwtService.GenerateAccessToken(new List<Claim>
        {
            new("userid",user.Id.ToString()),
            new ("display_name", user.Username),
            new("access_role", ((int)user.Role).ToString()),
            new("email", user.Email),
            new("remember", extended.ToString()),
            new("session_id",newSession.SessionId.ToString()!)
        });
        return new TokensDTO
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.ToString(),
        };
    }
    
}