using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace CoolAuth.Data.Entities;

[Table("sessions")]
public sealed class Session
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public Guid SessionId { get; set; }
    
    [Required(AllowEmptyStrings = false)]
    [Column("refresh_token")]
    public required string RefreshToken { get; set; }
    
    [Required]
    [Column("user_id")]
    public int UserId { get; set; }
    
    [Column("user_agent")]
    public string? UserAgent { get; set; }
    
    [Column("fingerprint")]
    public string? Fingerprint { get; set; }
    
    [Required]
    [Column("ip_address")]
    public required IPAddress IpAddress { get; set; }
    
    [Column("country")]
    public string? Country { get; set; }
    
    [Column("region")]
    public string? City { get; set; }
    
    [Column("latitude", TypeName = "decimal(8, 6)")]
    public decimal? Latitude { get; set; }
    
    [Column("longitude", TypeName = "decimal(9, 6)")]
    public decimal? Longitude { get; set; }
    
    [Required]
    [Column("issued_at")]
    public long IssuedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    
    [Required]
    [Column("expires_at")]
    public long ExpiresAt { get; set; }
    
    [Column("last_refresh_at")]
    public long LastRefreshAt { get; set; }
}