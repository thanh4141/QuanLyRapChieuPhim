using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBooking.DAL.Entities;

[Table("UserRefreshTokens")]
public class UserRefreshToken
{
    [Key]
    public int RefreshTokenId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(500)]
    public string RefreshToken { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}

