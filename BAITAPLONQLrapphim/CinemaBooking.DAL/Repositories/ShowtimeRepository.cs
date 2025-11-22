using CinemaBooking.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.DAL.Repositories;

public class ShowtimeRepository : Repository<Showtime>, IShowtimeRepository
{
    public ShowtimeRepository(CinemaDbContext context) : base(context)
    {
    }

    public async Task<List<Showtime>> GetShowtimesAsync(int? movieId = null, DateTime? date = null, int? auditoriumId = null, int skip = 0, int take = 10)
    {
        var query = _dbSet
            .Include(s => s.Movie)
            .Include(s => s.Auditorium)
            .Where(s => !s.IsDeleted && s.IsActive);

        if (movieId.HasValue)
        {
            query = query.Where(s => s.MovieId == movieId.Value);
        }

        if (date.HasValue)
        {
            var startOfDay = date.Value.Date;
            var endOfDay = startOfDay.AddDays(1);
            query = query.Where(s => s.StartTime >= startOfDay && s.StartTime < endOfDay);
        }

        if (auditoriumId.HasValue)
        {
            query = query.Where(s => s.AuditoriumId == auditoriumId.Value);
        }

        query = query.OrderBy(s => s.StartTime);

        return await query.Skip(skip).Take(take).ToListAsync();
    }

    public async Task<int> CountShowtimesAsync(int? movieId = null, DateTime? date = null, int? auditoriumId = null)
    {
        var query = _dbSet.Where(s => !s.IsDeleted && s.IsActive);

        if (movieId.HasValue)
        {
            query = query.Where(s => s.MovieId == movieId.Value);
        }

        if (date.HasValue)
        {
            var startOfDay = date.Value.Date;
            var endOfDay = startOfDay.AddDays(1);
            query = query.Where(s => s.StartTime >= startOfDay && s.StartTime < endOfDay);
        }

        if (auditoriumId.HasValue)
        {
            query = query.Where(s => s.AuditoriumId == auditoriumId.Value);
        }

        return await query.CountAsync();
    }

    public async Task<Showtime?> GetShowtimeWithDetailsAsync(int showtimeId)
    {
        return await _dbSet
            .Include(s => s.Movie)
            .Include(s => s.Auditorium)
            .FirstOrDefaultAsync(s => s.ShowtimeId == showtimeId && !s.IsDeleted);
    }

    public async Task<bool> HasOverlappingShowtimeAsync(int auditoriumId, DateTime startTime, DateTime endTime, int? excludeShowtimeId = null)
    {
        var query = _dbSet.Where(s => 
            s.AuditoriumId == auditoriumId &&
            !s.IsDeleted &&
            s.IsActive &&
            ((s.StartTime < endTime && s.EndTime > startTime)));

        if (excludeShowtimeId.HasValue)
        {
            query = query.Where(s => s.ShowtimeId != excludeShowtimeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<List<Showtime>> GetShowtimesByIdsAsync(List<int> showtimeIds)
    {
        return await _dbSet
            .Include(s => s.Movie)
            .Include(s => s.Auditorium)
            .Where(s => showtimeIds.Contains(s.ShowtimeId) && !s.IsDeleted)
            .ToListAsync();
    }
}

