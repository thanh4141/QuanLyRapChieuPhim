using AutoMapper;
using CinemaBooking.Common;
using CinemaBooking.Common.DTOs;
using CinemaBooking.DAL.Entities;
using CinemaBooking.DAL.Repositories;

namespace CinemaBooking.BLL.Services;

public class MovieService : IMovieService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MovieService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<MovieDto>> GetMoviesAsync(PagedRequest request, int? genreId = null, int? minDuration = null, int? maxDuration = null, decimal? minRating = null)
    {
        var skip = (request.PageIndex - 1) * request.PageSize;
        var movies = await _unitOfWork.Movies.GetMoviesWithGenresAsync(
            search: request.Search,
            genreId: genreId,
            minDuration: minDuration,
            maxDuration: maxDuration,
            minRating: minRating,
            sortBy: request.SortBy,
            sortDirection: request.SortDirection ?? "asc",
            skip: skip,
            take: request.PageSize
        );

        var totalItems = await _unitOfWork.Movies.CountMoviesAsync(
            search: request.Search,
            genreId: genreId,
            minDuration: minDuration,
            maxDuration: maxDuration,
            minRating: minRating
        );

        var movieDtos = _mapper.Map<List<MovieDto>>(movies);

        return new PagedResult<MovieDto>
        {
            Items = movieDtos,
            TotalItems = totalItems,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };
    }

    public async Task<MovieDto?> GetMovieByIdAsync(int movieId)
    {
        var movie = await _unitOfWork.Movies.GetMovieWithGenresAsync(movieId);
        if (movie == null) return null;
        return _mapper.Map<MovieDto>(movie);
    }

    public async Task<MovieDto> CreateMovieAsync(CreateMovieRequest request, int userId)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new Exception("Tên phim là bắt buộc");
        }

        if (request.DurationMinutes <= 0)
        {
            throw new Exception("Thời lượng phim phải lớn hơn 0");
        }

        var movie = _mapper.Map<Movie>(request);
        movie.CreatedBy = userId;
        movie.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Movies.AddAsync(movie);
        await _unitOfWork.SaveChangesAsync();

        // Add genres if provided
        if (request.GenreIds != null && request.GenreIds.Count > 0)
        {
            await _unitOfWork.Movies.AddMovieGenresAsync(movie.MovieId, request.GenreIds);
            await _unitOfWork.SaveChangesAsync();
        }

        // Log audit
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            UserId = userId,
            ActionName = "CreateMovie",
            EntityName = "Movie",
            EntityId = movie.MovieId.ToString(),
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        return await GetMovieByIdAsync(movie.MovieId) ?? _mapper.Map<MovieDto>(movie);
    }

    public async Task<MovieDto?> UpdateMovieAsync(int movieId, UpdateMovieRequest request, int userId)
    {
        var movie = await _unitOfWork.Movies.GetByIdAsync(movieId);
        if (movie == null || movie.IsDeleted) return null;

        _mapper.Map(request, movie);
        movie.UpdatedBy = userId;
        movie.UpdatedAt = DateTime.UtcNow;

        // Update genres - remove old and add new
        var existingGenres = await _unitOfWork.Movies.FindAsync(m => m.MovieId == movieId);
        // Need to implement genre update logic

        _unitOfWork.Movies.Update(movie);
        await _unitOfWork.SaveChangesAsync();

        // Log audit
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            UserId = userId,
            ActionName = "UpdateMovie",
            EntityName = "Movie",
            EntityId = movieId.ToString(),
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        return await GetMovieByIdAsync(movieId);
    }

    public async Task<bool> DeleteMovieAsync(int movieId, int userId)
    {
        var movie = await _unitOfWork.Movies.GetByIdAsync(movieId);
        if (movie == null || movie.IsDeleted) return false;

        movie.IsDeleted = true;
        movie.UpdatedBy = userId;
        movie.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Movies.Update(movie);
        await _unitOfWork.SaveChangesAsync();

        // Log audit
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            UserId = userId,
            ActionName = "DeleteMovie",
            EntityName = "Movie",
            EntityId = movieId.ToString(),
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<List<GenreDto>> GetAllGenresAsync()
    {
        var genres = await _unitOfWork.Genres.FindAsync(g => !g.IsDeleted);
        return _mapper.Map<List<GenreDto>>(genres);
    }
}

