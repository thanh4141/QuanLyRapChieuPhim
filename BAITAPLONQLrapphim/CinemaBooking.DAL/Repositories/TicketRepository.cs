using CinemaBooking.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.DAL.Repositories;

public class TicketRepository : Repository<Ticket>, ITicketRepository
{
    public TicketRepository(CinemaDbContext context) : base(context)
    {
    }

    public async Task<Ticket?> GetTicketByQrCodeAsync(string qrCodeData)
    {
        return await _dbSet
            .Include(t => t.Showtime)
                .ThenInclude(s => s!.Movie)
            .Include(t => t.Showtime)
                .ThenInclude(s => s!.Auditorium)
            .Include(t => t.Seat)
                .ThenInclude(s => s!.SeatType)
            .Include(t => t.Reservation)
                .ThenInclude(r => r!.User)
            .FirstOrDefaultAsync(t => t.QrCodeData == qrCodeData && !t.IsDeleted);
    }

    public async Task<Ticket?> GetTicketWithDetailsAsync(int ticketId)
    {
        return await _dbSet
            .Include(t => t.Showtime)
                .ThenInclude(s => s!.Movie)
            .Include(t => t.Showtime)
                .ThenInclude(s => s!.Auditorium)
            .Include(t => t.Seat)
                .ThenInclude(s => s!.SeatType)
            .Include(t => t.Reservation)
            .FirstOrDefaultAsync(t => t.TicketId == ticketId && !t.IsDeleted);
    }

    public async Task<List<Ticket>> GetUserTicketsAsync(int userId)
    {
        return await _dbSet
            .Include(t => t.Showtime)
                .ThenInclude(s => s!.Movie)
            .Include(t => t.Showtime)
                .ThenInclude(s => s!.Auditorium)
            .Include(t => t.Seat)
                .ThenInclude(s => s!.SeatType)
            .Include(t => t.Reservation)
            .Where(t => t.Reservation != null && t.Reservation.UserId == userId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Ticket>> GetTicketsByReservationAsync(int reservationId)
    {
        return await _dbSet
            .Include(t => t.Showtime)
                .ThenInclude(s => s!.Movie)
            .Include(t => t.Showtime)
                .ThenInclude(s => s!.Auditorium)
            .Include(t => t.Seat)
                .ThenInclude(s => s!.SeatType)
            .Include(t => t.Reservation)
            .Where(t => t.ReservationId == reservationId && !t.IsDeleted)
            .OrderBy(t => t.SeatId)
            .ToListAsync();
    }

    public async Task<List<Ticket>> GetBookedTicketsForShowtimeAsync(int showtimeId)
    {
        return await _dbSet
            .Include(t => t.Seat)
                .ThenInclude(s => s!.SeatType)
            .Where(t => t.ShowtimeId == showtimeId && 
                       (t.Status == "Booked" || t.Status == "Paid" || t.Status == "CheckedIn") &&
                       !t.IsDeleted)
            .ToListAsync();
    }
}

