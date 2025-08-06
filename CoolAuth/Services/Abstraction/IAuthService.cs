using CoolAuth.DTOs;
using CoolAuth.Requests;

namespace CoolAuth.Services;

public interface IAuthService
{
    Task<TokensDTO> LoginAsync(LoginRequest request);
    Task<TokensDTO> SignUpAsync(SignUpRequest request);
    Task SignOutAsync(TokensDTO tokens);
    Task<TokensDTO> RefreshSessionAsync(TokensDTO tokens);
    Task RevokeAsync(bool all, Guid sessionId, int userId);
}