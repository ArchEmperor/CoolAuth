using CoolAuth.Data.Entities;

namespace CoolAuth.Repositories;

public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(Guid sessionId);
    
    Task<Session?> GetByRefreshTokenAsync(string refreshToken);

    Task<IReadOnlyList<Session>> GetAllSessionsAsync(int userId);

    Task<Session> AddAsync(Session session);

    Task<Session> UpdateAsync(Session session);

    Task DeleteAsync(Guid sessionId);

    Task DeleteAllSessionsAsync(int userId);

    Task<IReadOnlyList<Session>> GetExpiredAsync(long currentUnixTimeSeconds);
}