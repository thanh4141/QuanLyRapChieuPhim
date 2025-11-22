using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBooking.DAL.Entities;

[Table("MovieGenres")]
public class MovieGenre
{
    [Key]
    public int MovieGenreId { get; set; }

    [Required]
    public int MovieId { get; set; }

    [Required]
    public int GenreId { get; set; }

    // Navigation properties
    [ForeignKey("MovieId")]
    public virtual Movie Movie { get; set; } = null!;

    [ForeignKey("GenreId")]
    public virtual Genre Genre { get; set; } = null!;
}

