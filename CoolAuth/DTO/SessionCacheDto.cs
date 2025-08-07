namespace CoolAuth.DTO;

public class SessionCacheDto
{
    public Guid SessionId { get; set; }
    public string RefreshToken { get; set; } = default!;
    public int UserId { get; set; }
    public string? UserAgent { get; set; }
    public string? Fingerprint { get; set; }
    public string IpAddress { get; set; } = default!;
    public long IssuedAt { get; set; }
    public long ExpiresAt { get; set; }
    public long LastRefreshAt { get; set; }
}