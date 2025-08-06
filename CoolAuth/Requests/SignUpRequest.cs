using System.ComponentModel.DataAnnotations;

namespace CoolAuth.Requests;

[Serializable]
public class SignUpRequest
{
    [StringLength(128,MinimumLength = 3)]
    public required string Username { get; init; }
    
    [EmailAddress]
    public required string Email { get; init; }
    
    [StringLength(128,MinimumLength = 8)]
    public required string Password { get; init; }
}