using CinemaBooking.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.DAL.Repositories;

public class BookingRepository : Repository<Reservation>, IBookingRepository
{
    public BookingRepository(CinemaDbContext context) : base(context)
    {
    }

    public async Task<Reservation?> GetReservationWithDetailsAsync(int reservationId)
    {
        return await _dbSet
            .Include(r => r.User)
            .Include(r => r.Showtime)
                .ThenInclude(s => s!.Movie)
            .Include(r => r.Showtime)
                .ThenInclude(s => s!.Auditorium)
            .Include(r => r.Tickets)
                .ThenInclude(t => t.Seat)
                    .ThenInclude(s => s!.SeatType)
            .Include(r => r.Invoices)
                .ThenInclude(i => i.Payments)
            .FirstOrDefaultAsync(r => r.ReservationId == reservationId && !r.IsDeleted);
    }

    public async Task<List<Reservation>> GetUserReservationsAsync(int userId, int skip = 0, int take = 10)
    {
        return await _dbSet
            .Include(r => r.Showtime)
                .ThenInclude(s => s!.Movie)
            .Include(r => r.Showtime)
                .ThenInclude(s => s!.Auditorium)
            .Include(r => r.Tickets)
            .Where(r => r.UserId == userId && !r.IsDeleted)
            .OrderByDescending(r => r.ReservedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> CountUserReservationsAsync(int userId)
    {
        return await _dbSet
            .Where(r => r.UserId == userId && !r.IsDeleted)
            .CountAsync();
    }

    public async Task<bool> AreSeatsAvailableAsync(int showtimeId, List<int> seatIds)
    {
        var bookedSeatIds = await _context.Tickets
            .Where(t => t.ShowtimeId == showtimeId && 
                       seatIds.Contains(t.SeatId) &&
                       (t.Status == "Booked" || t.Status == "Paid" || t.Status == "CheckedIn") &&
                       !t.IsDeleted)
            .Select(t => t.SeatId)
            .ToListAsync();

        return !bookedSeatIds.Any();
    }

    public async Task<List<Ticket>> GetBookedTicketsForShowtimeAsync(int showtimeId)
    {
        return await _context.Tickets
            .Include(t => t.Seat)
                .ThenInclude(s => s!.SeatType)
            .Where(t => t.ShowtimeId == showtimeId && 
                       (t.Status == "Booked" || t.Status == "Paid" || t.Status == "CheckedIn") &&
                       !t.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<Reservation>> GetReservationsByIdsAsync(List<int> reservationIds)
    {
        return await _dbSet
            .Where(r => reservationIds.Contains(r.ReservationId) && !r.IsDeleted)
            .ToListAsync();
    }
}

