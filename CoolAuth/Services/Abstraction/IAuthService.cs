using CoolAuth.DTOs;
using CoolAuth.Requests;

namespace CoolAuth.Services.Abstraction;

public interface IAuthService
{
    Task<TokensDTO> LoginAsync(LoginRequest request,SessionConnectionInfoDTO info);
    public Task<TokensDTO> SignUpAsync(SignUpRequest request, SessionConnectionInfoDTO info);
    Task SignOutAsync(string refreshToken);
    public Task<TokensDTO> RefreshSessionAsync(RefreshSessionRequest request,SessionConnectionInfoDTO info);
    Task RevokeAsync(HttpContext context,bool all=false, Guid? sessionId=null);
}