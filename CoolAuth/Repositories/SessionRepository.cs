using CoolAuth.Data;
using CoolAuth.Data.Entities;
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

    public async Task<IReadOnlyList<Session>> GetAllSessionsAsync(int userId)
    {
        return await context.Sessions
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }

    public async Task AddAsync(Session session)
    {
        await context.Sessions.AddAsync(session);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Session session)
    {
        context.Sessions.Update(session);
        await context.SaveChangesAsync();
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