using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBooking.DAL.Entities;

[Table("Showtimes")]
public class Showtime
{
    [Key]
    public int ShowtimeId { get; set; }

    [Required]
    public int MovieId { get; set; }

    [Required]
    public int AuditoriumId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal BaseTicketPrice { get; set; }

    [MaxLength(50)]
    public string? Language { get; set; }

    [MaxLength(50)]
    public string? Format { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    [ForeignKey("MovieId")]
    public virtual Movie Movie { get; set; } = null!;

    [ForeignKey("AuditoriumId")]
    public virtual Auditorium Auditorium { get; set; } = null!;

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}

