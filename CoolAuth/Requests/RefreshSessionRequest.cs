namespace CoolAuth.Requests;

public class RefreshSessionRequest
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public string? Fingerprint { get; set; }
}