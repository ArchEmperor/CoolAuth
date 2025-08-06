using System.Net;

namespace CoolAuth.DTOs;

public class SessionConnectionInfoDTO
{
    public string? UserAgent { get; set; }
    public required IPAddress IpAddress { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}