namespace CinemaBooking.Common.DTOs;

public class MovieDto
{
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? OriginalTitle { get; set; }
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public string? AgeRating { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? Country { get; set; }
    public string? Director { get; set; }
    public string? Cast { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public decimal? ImdbRating { get; set; }
    public List<GenreDto> Genres { get; set; } = new();
}

public class CreateMovieRequest
{
    public string Title { get; set; } = string.Empty;
    public string? OriginalTitle { get; set; }
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public string? AgeRating { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? Country { get; set; }
    public string? Director { get; set; }
    public string? Cast { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public decimal? ImdbRating { get; set; }
    public List<int> GenreIds { get; set; } = new();
}

public class UpdateMovieRequest
{
    public string Title { get; set; } = string.Empty;
    public string? OriginalTitle { get; set; }
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public string? AgeRating { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? Country { get; set; }
    public string? Director { get; set; }
    public string? Cast { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public decimal? ImdbRating { get; set; }
    public List<int> GenreIds { get; set; } = new();
}

public class GenreDto
{
    public int GenreId { get; set; }
    public string GenreName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

