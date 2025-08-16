using CoolAuth.DTO;
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
    public Task<IReadOnlyList<SessionPartialDto>> GetSessionsAsync(HttpContext context);
    public Task<string> GenerateMagicTokenAsync(HttpContext context,bool remember=false);
    public Task<TokensDTO> MagicLoginAsync(MagicLoginRequest request,SessionConnectionInfoDTO info);
}