using CoolAuth.Data.Entities;
using CoolAuth.DTO;

namespace CoolAuth.Repositories;

public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(Guid sessionId);
    
    Task<Session?> GetByRefreshTokenAsync(string refreshToken);
    public Task<IReadOnlyList<Guid>> GetAllSessionsIdsAsync(int userId);

    Task<IReadOnlyList<SessionPartialDto>> GetAllSessionsAsync(int userId);

    Task<Session> AddAsync(Session session);

    Task<Session> UpdateAsync(Session session);

    Task DeleteAsync(Guid sessionId);

    Task DeleteAllSessionsAsync(int userId);

    Task<IReadOnlyList<Session>> GetExpiredAsync(long currentUnixTimeSeconds);
}