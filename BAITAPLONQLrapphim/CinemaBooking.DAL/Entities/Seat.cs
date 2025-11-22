using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBooking.DAL.Entities;

[Table("Seats")]
public class Seat
{
    [Key]
    public int SeatId { get; set; }

    [Required]
    public int AuditoriumId { get; set; }

    [Required]
    [MaxLength(10)]
    public string RowLabel { get; set; } = string.Empty;

    [Required]
    public int SeatNumber { get; set; }

    [Required]
    public int SeatTypeId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    [ForeignKey("AuditoriumId")]
    public virtual Auditorium Auditorium { get; set; } = null!;

    [ForeignKey("SeatTypeId")]
    public virtual SeatType SeatType { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}

