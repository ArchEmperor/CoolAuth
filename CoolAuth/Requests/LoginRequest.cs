using System.ComponentModel.DataAnnotations;

namespace CoolAuth.Requests;

public class LoginRequest
{
    [EmailAddress]
    public required string Email { get; set; }
    [StringLength(128,MinimumLength = 8)]
    public required string Password { get; set; }
    public required bool Remember { get; set; }
    public string? Fingerprint { get; set; }
}