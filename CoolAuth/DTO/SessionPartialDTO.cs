using System.Net;

namespace CoolAuth.DTO;

public class SessionPartialDto
{
    public Guid SessionId { get; set; }
    public string? UserAgent { get; set; }
    public required IPAddress IpAddress { get; set; }
    public long ExpiresAt { get; set; }
    public long LastRefreshAt { get; set; }
}