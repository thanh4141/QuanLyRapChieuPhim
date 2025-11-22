using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBooking.DAL.Entities;

[Table("AuditLogs")]
public class AuditLog
{
    [Key]
    public long AuditLogId { get; set; }

    public int? UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ActionName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? EntityName { get; set; }

    [MaxLength(100)]
    public string? EntityId { get; set; }

    public string? Details { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? IPAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}

