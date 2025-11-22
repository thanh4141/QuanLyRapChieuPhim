using CinemaBooking.DAL.Entities;

namespace CinemaBooking.DAL.Repositories;

public interface IMovieRepository : IRepository<Movie>
{
    Task<List<Movie>> GetMoviesWithGenresAsync(string? search = null, int? genreId = null, int? minDuration = null, int? maxDuration = null, decimal? minRating = null, string? sortBy = null, string? sortDirection = "asc", int skip = 0, int take = 10);
    Task<int> CountMoviesAsync(string? search = null, int? genreId = null, int? minDuration = null, int? maxDuration = null, decimal? minRating = null);
    Task<Movie?> GetMovieWithGenresAsync(int movieId);
    Task<List<Movie>> GetMoviesByIdsAsync(List<int> movieIds);
    Task AddMovieGenresAsync(int movieId, List<int> genreIds);
}

