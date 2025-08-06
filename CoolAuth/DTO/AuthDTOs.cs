namespace CoolAuth.DTOs;

public class TokensDTO
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}

