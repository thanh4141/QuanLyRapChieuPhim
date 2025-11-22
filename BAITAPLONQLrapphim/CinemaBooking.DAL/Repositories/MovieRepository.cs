using CinemaBooking.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.DAL.Repositories;

public class MovieRepository : Repository<Movie>, IMovieRepository
{
    public MovieRepository(CinemaDbContext context) : base(context)
    {
    }

    public async Task<List<Movie>> GetMoviesWithGenresAsync(string? search = null, int? genreId = null, int? minDuration = null, int? maxDuration = null, decimal? minRating = null, string? sortBy = null, string? sortDirection = "asc", int skip = 0, int take = 10)
    {
        var query = _dbSet
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .Where(m => !m.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m => m.Title.Contains(search) || 
                                     (m.OriginalTitle != null && m.OriginalTitle.Contains(search)) ||
                                     (m.Description != null && m.Description.Contains(search)));
        }

        if (genreId.HasValue)
        {
            query = query.Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId.Value));
        }

        if (minDuration.HasValue)
        {
            query = query.Where(m => m.DurationMinutes >= minDuration.Value);
        }

        if (maxDuration.HasValue)
        {
            query = query.Where(m => m.DurationMinutes <= maxDuration.Value);
        }

        if (minRating.HasValue)
        {
            query = query.Where(m => m.ImdbRating != null && m.ImdbRating >= minRating.Value);
        }

        // Sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            query = sortBy.ToLower() switch
            {
                "title" => sortDirection == "desc" ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title),
                "releasedate" => sortDirection == "desc" ? query.OrderByDescending(m => m.ReleaseDate) : query.OrderBy(m => m.ReleaseDate),
                "rating" => sortDirection == "desc" ? query.OrderByDescending(m => m.ImdbRating) : query.OrderBy(m => m.ImdbRating),
                "duration" => sortDirection == "desc" ? query.OrderByDescending(m => m.DurationMinutes) : query.OrderBy(m => m.DurationMinutes),
                _ => query.OrderBy(m => m.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(m => m.CreatedAt);
        }

        return await query.Skip(skip).Take(take).ToListAsync();
    }

    public async Task<int> CountMoviesAsync(string? search = null, int? genreId = null, int? minDuration = null, int? maxDuration = null, decimal? minRating = null)
    {
        var query = _dbSet.Where(m => !m.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m => m.Title.Contains(search) || 
                                     (m.OriginalTitle != null && m.OriginalTitle.Contains(search)) ||
                                     (m.Description != null && m.Description.Contains(search)));
        }

        if (genreId.HasValue)
        {
            query = query.Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId.Value));
        }

        if (minDuration.HasValue)
        {
            query = query.Where(m => m.DurationMinutes >= minDuration.Value);
        }

        if (maxDuration.HasValue)
        {
            query = query.Where(m => m.DurationMinutes <= maxDuration.Value);
        }

        if (minRating.HasValue)
        {
            query = query.Where(m => m.ImdbRating != null && m.ImdbRating >= minRating.Value);
        }

        return await query.CountAsync();
    }

    public async Task<Movie?> GetMovieWithGenresAsync(int movieId)
    {
        return await _dbSet
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .FirstOrDefaultAsync(m => m.MovieId == movieId && !m.IsDeleted);
    }

    public async Task<List<Movie>> GetMoviesByIdsAsync(List<int> movieIds)
    {
        return await _dbSet
            .Where(m => movieIds.Contains(m.MovieId) && !m.IsDeleted)
            .ToListAsync();
    }

    public async Task AddMovieGenresAsync(int movieId, List<int> genreIds)
    {
        var movieGenres = genreIds.Select(genreId => new MovieGenre
        {
            MovieId = movieId,
            GenreId = genreId
        }).ToList();

        await _context.MovieGenres.AddRangeAsync(movieGenres);
    }
}

