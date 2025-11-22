using AutoMapper;
using CinemaBooking.Common;
using CinemaBooking.Common.DTOs;
using CinemaBooking.DAL.Entities;
using CinemaBooking.DAL.Repositories;

namespace CinemaBooking.BLL.Services;

public class ShowtimeService : IShowtimeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ShowtimeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<ShowtimeDto>> GetShowtimesAsync(PagedRequest request, int? movieId = null, DateTime? date = null, int? auditoriumId = null)
    {
        var skip = (request.PageIndex - 1) * request.PageSize;
        var showtimes = await _unitOfWork.Showtimes.GetShowtimesAsync(movieId, date, auditoriumId, skip, request.PageSize);
        var totalItems = await _unitOfWork.Showtimes.CountShowtimesAsync(movieId, date, auditoriumId);

        var showtimeDtos = _mapper.Map<List<ShowtimeDto>>(showtimes);

        return new PagedResult<ShowtimeDto>
        {
            Items = showtimeDtos,
            TotalItems = totalItems,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };
    }

    public async Task<ShowtimeDto?> GetShowtimeByIdAsync(int showtimeId)
    {
        var showtime = await _unitOfWork.Showtimes.GetShowtimeWithDetailsAsync(showtimeId);
        if (showtime == null) return null;
        return _mapper.Map<ShowtimeDto>(showtime);
    }

    public async Task<ShowtimeDto> CreateShowtimeAsync(CreateShowtimeRequest request, int userId)
    {
        var movie = await _unitOfWork.Movies.GetByIdAsync(request.MovieId);
        if (movie == null || movie.IsDeleted)
        {
            throw new Exception("Không tìm thấy phim");
        }

        var auditorium = await _unitOfWork.Auditoriums.GetByIdAsync(request.AuditoriumId);
        if (auditorium == null || auditorium.IsDeleted || !auditorium.IsActive)
        {
            throw new Exception("Không tìm thấy phòng chiếu hoặc phòng chiếu không hoạt động");
        }

        // Calculate end time (assuming movie duration)
        var endTime = request.StartTime.AddMinutes(movie.DurationMinutes);

        // Check for overlapping showtimes
        var hasOverlap = await _unitOfWork.Showtimes.HasOverlappingShowtimeAsync(
            request.AuditoriumId, 
            request.StartTime, 
            endTime
        );

        if (hasOverlap)
        {
            throw new Exception("Suất chiếu bị trùng với suất chiếu khác trong cùng phòng");
        }

        var showtime = _mapper.Map<Showtime>(request);
        showtime.EndTime = endTime;
        showtime.CreatedBy = userId;
        showtime.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Showtimes.AddAsync(showtime);
        await _unitOfWork.SaveChangesAsync();

        // Log audit
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            UserId = userId,
            ActionName = "CreateShowtime",
            EntityName = "Showtime",
            EntityId = showtime.ShowtimeId.ToString(),
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        return await GetShowtimeByIdAsync(showtime.ShowtimeId) ?? _mapper.Map<ShowtimeDto>(showtime);
    }

    public async Task<ShowtimeDto?> UpdateShowtimeAsync(int showtimeId, UpdateShowtimeRequest request, int userId)
    {
        var showtime = await _unitOfWork.Showtimes.GetByIdAsync(showtimeId);
        if (showtime == null || showtime.IsDeleted)
        {
            return null;
        }

        var movie = await _unitOfWork.Movies.GetByIdAsync(request.MovieId);
        if (movie == null || movie.IsDeleted)
        {
            throw new Exception("Không tìm thấy phim");
        }

        var auditorium = await _unitOfWork.Auditoriums.GetByIdAsync(request.AuditoriumId);
        if (auditorium == null || auditorium.IsDeleted || !auditorium.IsActive)
        {
            throw new Exception("Không tìm thấy phòng chiếu hoặc phòng chiếu không hoạt động");
        }

        var endTime = request.StartTime.AddMinutes(movie.DurationMinutes);

        // Check for overlapping showtimes (excluding current)
        var hasOverlap = await _unitOfWork.Showtimes.HasOverlappingShowtimeAsync(
            request.AuditoriumId,
            request.StartTime,
            endTime,
            showtimeId
        );

        if (hasOverlap)
        {
            throw new Exception("Suất chiếu bị trùng với suất chiếu khác trong cùng phòng");
        }

        _mapper.Map(request, showtime);
        showtime.EndTime = endTime;
        showtime.UpdatedBy = userId;
        showtime.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Showtimes.Update(showtime);
        await _unitOfWork.SaveChangesAsync();

        // Log audit
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            UserId = userId,
            ActionName = "UpdateShowtime",
            EntityName = "Showtime",
            EntityId = showtimeId.ToString(),
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        return await GetShowtimeByIdAsync(showtimeId);
    }

    public async Task<bool> DeleteShowtimeAsync(int showtimeId, int userId)
    {
        var showtime = await _unitOfWork.Showtimes.GetByIdAsync(showtimeId);
        if (showtime == null || showtime.IsDeleted)
        {
            return false;
        }

        showtime.IsDeleted = true;
        showtime.UpdatedBy = userId;
        showtime.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Showtimes.Update(showtime);
        await _unitOfWork.SaveChangesAsync();

        // Log audit
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            UserId = userId,
            ActionName = "DeleteShowtime",
            EntityName = "Showtime",
            EntityId = showtimeId.ToString(),
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

