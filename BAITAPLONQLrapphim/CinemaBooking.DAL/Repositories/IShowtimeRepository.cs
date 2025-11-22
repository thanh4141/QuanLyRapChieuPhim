using CinemaBooking.DAL.Entities;

namespace CinemaBooking.DAL.Repositories;

public interface IShowtimeRepository : IRepository<Showtime>
{
    Task<List<Showtime>> GetShowtimesAsync(int? movieId = null, DateTime? date = null, int? auditoriumId = null, int skip = 0, int take = 10);
    Task<int> CountShowtimesAsync(int? movieId = null, DateTime? date = null, int? auditoriumId = null);
    Task<Showtime?> GetShowtimeWithDetailsAsync(int showtimeId);
    Task<bool> HasOverlappingShowtimeAsync(int auditoriumId, DateTime startTime, DateTime endTime, int? excludeShowtimeId = null);
    Task<List<Showtime>> GetShowtimesByIdsAsync(List<int> showtimeIds);
}

