using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBooking.DAL.Entities;

[Table("Auditoriums")]
public class Auditorium
{
    [Key]
    public int AuditoriumId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LocationDescription { get; set; }

    [Required]
    public int Capacity { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}

