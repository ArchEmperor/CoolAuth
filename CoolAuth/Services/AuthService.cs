using CoolAuth.Data.Entities;
using CoolAuth.Domain;
using CoolAuth.DTOs;
using CoolAuth.Repositories;
using CoolAuth.Requests;

namespace CoolAuth.Services;

public class AuthService(UserRepository users, SessionRepository sessions, JwtService jwtAuth) : IAuthService
{
    public Task<TokensDTO> LoginAsync(LoginRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<TokensDTO> SignUpAsync(SignUpRequest request)
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
        var tokens = jwtAuth.CreateNewSession(user,false);
            
        return;

    }

    public Task SignOutAsync(TokensDTO tokens)
    {
        throw new NotImplementedException();
    }

    public Task<TokensDTO> RefreshSessionAsync(TokensDTO tokens)
    {
        throw new NotImplementedException();
    }

    public Task RevokeAsync(bool all, Guid sessionId, int userId)
    {
        throw new NotImplementedException();
    }
}