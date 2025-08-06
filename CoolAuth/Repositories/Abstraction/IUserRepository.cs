using CoolAuth.Data.Entities;

namespace CoolAuth.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    
    Task<User?> GetByEmailAsync(string email);

    Task<bool> ExistsByEmailAsync(string email);

    Task<User> AddAsync(User user);

    Task UpdateAsync(User user);
    
    Task DeleteAsync(int  userId);
}