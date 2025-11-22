using CinemaBooking.Common;
using CinemaBooking.Common.DTOs;

namespace CinemaBooking.BLL.Services;

public interface IShowtimeService
{
    Task<PagedResult<ShowtimeDto>> GetShowtimesAsync(PagedRequest request, int? movieId = null, DateTime? date = null, int? auditoriumId = null);
    Task<ShowtimeDto?> GetShowtimeByIdAsync(int showtimeId);
    Task<ShowtimeDto> CreateShowtimeAsync(CreateShowtimeRequest request, int userId);
    Task<ShowtimeDto?> UpdateShowtimeAsync(int showtimeId, UpdateShowtimeRequest request, int userId);
    Task<bool> DeleteShowtimeAsync(int showtimeId, int userId);
}

