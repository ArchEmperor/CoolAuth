namespace CoolAuth.Requests;

public class MagicLoginRequest
{
    public required string Token { get; set; }
    public string? Fingerprint { get; set; }
}