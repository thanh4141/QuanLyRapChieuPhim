using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBooking.DAL.Entities;

[Table("SeatTypes")]
public class SeatType
{
    [Key]
    public int SeatTypeId { get; set; }

    [Required]
    [MaxLength(50)]
    public string SeatTypeName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal PriceMultiplier { get; set; } = 1.0m;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
}

