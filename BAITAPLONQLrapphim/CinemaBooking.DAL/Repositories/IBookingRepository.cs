using CinemaBooking.DAL.Entities;

namespace CinemaBooking.DAL.Repositories;

public interface IBookingRepository : IRepository<Reservation>
{
    Task<Reservation?> GetReservationWithDetailsAsync(int reservationId);
    Task<List<Reservation>> GetUserReservationsAsync(int userId, int skip = 0, int take = 10);
    Task<int> CountUserReservationsAsync(int userId);
    Task<bool> AreSeatsAvailableAsync(int showtimeId, List<int> seatIds);
    Task<List<Ticket>> GetBookedTicketsForShowtimeAsync(int showtimeId);
    Task<List<Reservation>> GetReservationsByIdsAsync(List<int> reservationIds);
}

