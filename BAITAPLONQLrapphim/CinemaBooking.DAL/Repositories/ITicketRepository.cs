using CinemaBooking.DAL.Entities;

namespace CinemaBooking.DAL.Repositories;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<Ticket?> GetTicketByQrCodeAsync(string qrCodeData);
    Task<Ticket?> GetTicketWithDetailsAsync(int ticketId);
    Task<List<Ticket>> GetUserTicketsAsync(int userId);
    Task<List<Ticket>> GetTicketsByReservationAsync(int reservationId);
    Task<List<Ticket>> GetBookedTicketsForShowtimeAsync(int showtimeId);
}

