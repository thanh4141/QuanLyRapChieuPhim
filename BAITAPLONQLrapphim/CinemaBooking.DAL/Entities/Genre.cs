using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBooking.DAL.Entities;

[Table("Genres")]
public class Genre
{
    [Key]
    public int GenreId { get; set; }

    [Required]
    [MaxLength(100)]
    public string GenreName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public virtual ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
}

