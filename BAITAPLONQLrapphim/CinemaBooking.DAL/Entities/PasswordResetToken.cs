using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBooking.DAL.Entities;

[Table("PasswordResetTokens")]
public class PasswordResetToken
{
    [Key]
    public int PasswordResetTokenId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Token { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}

