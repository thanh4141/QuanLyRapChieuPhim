using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBooking.DAL.Entities;

[Table("Tickets")]
public class Ticket
{
    [Key]
    public int TicketId { get; set; }

    [Required]
    public int ReservationId { get; set; }

    [Required]
    public int ShowtimeId { get; set; }

    [Required]
    public int SeatId { get; set; }

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal SeatPrice { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Booked"; // Booked, Paid, Cancelled, CheckedIn

    [Required]
    [MaxLength(255)]
    public string QrCodeData { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? QrCodeImageUrl { get; set; }

    public DateTime? CheckedInAt { get; set; }

    public int? CheckedInBy { get; set; }

    public int? InvoiceId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    [ForeignKey("ReservationId")]
    public virtual Reservation Reservation { get; set; } = null!;

    [ForeignKey("ShowtimeId")]
    public virtual Showtime Showtime { get; set; } = null!;

    [ForeignKey("SeatId")]
    public virtual Seat Seat { get; set; } = null!;

    [ForeignKey("CheckedInBy")]
    public virtual User? CheckedInByUser { get; set; }

    [ForeignKey("InvoiceId")]
    public virtual Invoice? Invoice { get; set; }
}

