namespace CoolAuth.DTOs;

public class TokensDTO
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}

public class MagicDTO
{
    public required int UserId { get; set; }
    public required bool Remember { get; set; }
}
