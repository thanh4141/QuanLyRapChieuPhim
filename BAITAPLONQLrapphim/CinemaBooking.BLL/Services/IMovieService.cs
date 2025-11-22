using CinemaBooking.Common;
using CinemaBooking.Common.DTOs;

namespace CinemaBooking.BLL.Services;

public interface IMovieService
{
    Task<PagedResult<MovieDto>> GetMoviesAsync(PagedRequest request, int? genreId = null, int? minDuration = null, int? maxDuration = null, decimal? minRating = null);
    Task<MovieDto?> GetMovieByIdAsync(int movieId);
    Task<MovieDto> CreateMovieAsync(CreateMovieRequest request, int userId);
    Task<MovieDto?> UpdateMovieAsync(int movieId, UpdateMovieRequest request, int userId);
    Task<bool> DeleteMovieAsync(int movieId, int userId);
    Task<List<GenreDto>> GetAllGenresAsync();
}

