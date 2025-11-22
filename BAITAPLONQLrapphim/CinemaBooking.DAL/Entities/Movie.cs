using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBooking.DAL.Entities;

[Table("Movies")]
public class Movie
{
    [Key]
    public int MovieId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? OriginalTitle { get; set; }

    public string? Description { get; set; }

    [Required]
    public int DurationMinutes { get; set; }

    [MaxLength(20)]
    public string? AgeRating { get; set; }

    public DateTime? ReleaseDate { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(200)]
    public string? Director { get; set; }

    [MaxLength(1000)]
    public string? Cast { get; set; }

    [MaxLength(500)]
    public string? PosterUrl { get; set; }

    [MaxLength(500)]
    public string? TrailerUrl { get; set; }

    [Column(TypeName = "decimal(3,1)")]
    public decimal? ImdbRating { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public virtual ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}

