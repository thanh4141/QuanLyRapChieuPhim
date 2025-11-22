using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBooking.DAL.Entities;

[Table("Reservations")]
public class Reservation
{
    [Key]
    public int ReservationId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int ShowtimeId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ReservationCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Expired

    [Required]
    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    [MaxLength(500)]
    public string? CancelReason { get; set; }

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal TotalAmount { get; set; } = 0;

    [MaxLength(500)]
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("ShowtimeId")]
    public virtual Showtime Showtime { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

