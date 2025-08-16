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

public class AuthService(IUserRepository users, ISessionRepository sessions, JwtService jwtService, UserSessionCacheService userCache) : IAuthService
{
    private const int RefreshTokenExtendedLifetime = 7; //days
    private const int RefreshTokenLifetime = 30;//minutes
    private const int MaxSessionsAmount = 5;
    public async Task<TokensDTO> LoginAsync(LoginRequest request,SessionConnectionInfoDTO info)
    {
        var user =await userCache.GetCachedUserByEmailAsync(request.Email) ??
                  await users.GetByEmailAsync(request.Email);
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
        await userCache.CacheUserAsync(user);
        return await CreateNewSession(user,false,info,request.Fingerprint);
    }
    public async Task SignOutAsync(string refreshToken)
    {
        Session? session = null;
        session = await userCache.GetCachedSessionByTokenAsync(refreshToken) ?? 
                  await sessions.GetByRefreshTokenAsync(refreshToken);
        if (session==null)
        {
            throw DomainException.InvalidAuthToken;
        }
        await sessions.DeleteAsync(session.SessionId);
        await userCache.DeleteCachedSessionAsync(session.SessionId);
    }

    public async Task<TokensDTO> RefreshSessionAsync(RefreshSessionRequest request,SessionConnectionInfoDTO info)
    {
        var accessToken = JwtService.ReadToken(request.AccessToken);
        if (accessToken == null || !Guid.TryParse(accessToken.Payload["session_id"].ToString(), out var sessionId))
        {
            throw DomainException.InvalidAuthToken;
        }

        Session? session = null;
        session = await userCache.GetCachedSessionByTokenAsync(request.RefreshToken) ?? 
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
            await userCache.DeleteCachedSessionAsync(sessionId.Value);
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
            await userCache.DeleteCachedSessionAsync(id);
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

    public async Task<string> GenerateMagicTokenAsync(HttpContext context,bool remember=false)
    {
        var token = Guid.NewGuid().ToString();
        if (!JwtService.GetUserIdFromHttpContext(context, out var userId))
        {
            throw DomainException.InvalidAuthToken;
        }

        var userInfo = new MagicDTO
        {
            UserId = userId,
            Remember = remember
        };

        await userCache.CacheMagicTokenAsync(token, userInfo);
        return token;
    }

    public async Task<TokensDTO> MagicLoginAsync(MagicLoginRequest request,SessionConnectionInfoDTO info)
    {
        var magicDto = await userCache.UseMagicTokenAsync(request.Token);

        if (magicDto is null)
        {
            throw DomainException.InvalidMagicToken;
        }
        var user =await userCache.GetCachedUserByIdAsync(magicDto.UserId.ToString())??
                  await users.GetByIdAsync(magicDto.UserId);
        if (user == null)
        { 
            throw DomainException.InvalidMagicToken;
        }
        return await CreateNewSession(user,magicDto.Remember,info,request.Fingerprint);
    }

    private async Task<TokensDTO> CreateNewSession(User user,bool extended,SessionConnectionInfoDTO info, string? fingerprint,Guid? sessionId = null)
    {
        var refreshToken = Guid.NewGuid();
        var expires= extended ? DateTimeOffset.UtcNow.AddDays(RefreshTokenExtendedLifetime) :
            DateTimeOffset.UtcNow.AddMinutes(RefreshTokenLifetime);
        
        var userSessions = await sessions.GetAllSessionsAsync(user.Id);
        if (userSessions.Count >= MaxSessionsAmount)
        {
            await sessions.DeleteAsync(userSessions[0].SessionId);
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
        await userCache.CacheSessionAsync(newSession, expires);
        
        var accessToken = jwtService.GenerateAccessToken(new List<Claim>
        {
            new("userid",user.Id.ToString()),
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