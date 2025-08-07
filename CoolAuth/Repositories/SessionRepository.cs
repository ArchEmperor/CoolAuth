using CoolAuth.Data;
using CoolAuth.Data.Entities;
using CoolAuth.DTO;
using Microsoft.EntityFrameworkCore;

namespace CoolAuth.Repositories;

public class SessionRepository(AppDbContext context): ISessionRepository
{
    public async Task<Session?> GetByIdAsync(Guid sessionId)
    {
        var session = await context.Sessions.FindAsync(sessionId);
        return session;
    }

    public async Task<Session?> GetByRefreshTokenAsync(string refreshToken)
    {
        var session = await context.Sessions.FirstOrDefaultAsync(o=>
            o.RefreshToken == refreshToken);
        return session;
    }

    public async Task<IReadOnlyList<SessionPartialDto>> GetAllSessionsAsync(int userId)
    {
        return await context.Sessions
            .Where(s => s.UserId == userId)
            .Select(s => new SessionPartialDto
            {
                SessionId = s.SessionId,
                UserAgent = s.UserAgent,
                IpAddress = s.IpAddress,
                ExpiresAt = s.ExpiresAt,
                LastRefreshAt = s.LastRefreshAt
            })
            .ToListAsync();
    }
    public async Task<IReadOnlyList<Guid>> GetAllSessionsIdsAsync(int userId)
    {
        return await context.Sessions
            .Where(s => s.UserId == userId)
            .Select(s => s.SessionId)
            .ToListAsync();
    }

    public async Task<Session> AddAsync(Session session)
    {
        var result = await context.Sessions.AddAsync(session);
        await context.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<Session> UpdateAsync(Session session)
    {
        var result = context.Sessions.Update(session);
        await context.SaveChangesAsync();
        return result.Entity;
    }

    public async Task DeleteAsync(Guid sessionId)
    {
        var entity = await context.Sessions.FindAsync(sessionId);
        if (entity != null)
        {
            context.Sessions.Remove(entity);
        }
        await context.SaveChangesAsync();
    }

    public async Task DeleteAllSessionsAsync(int userId)
    {
        context.Sessions.RemoveRange(context.Sessions.Where(s => 
            s.UserId == userId));
        await context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Session>> GetExpiredAsync(long currentUnixTimeSeconds)
    {
        return await context.Sessions.Where(o => 
            o.ExpiresAt < currentUnixTimeSeconds).ToListAsync();
    }
}