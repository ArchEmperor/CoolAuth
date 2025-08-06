using CoolAuth.Data;
using CoolAuth.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoolAuth.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(int id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await context.Users.FirstOrDefaultAsync(o=>
            o.Email==email);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await context.Users.AnyAsync(o => o.Email == email);
    }

    public async Task<User> AddAsync(User user)
    { 
        var result= await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        return result.Entity;
    }

    public async Task UpdateAsync(User user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int userId)
    {
        var entity = await context.Users.FindAsync(userId);
        if (entity != null)
        {
            context.Users.Remove(entity);
        }
        await context.SaveChangesAsync();
    }
}