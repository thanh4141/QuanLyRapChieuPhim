using CinemaBooking.DAL.Entities;

namespace CinemaBooking.DAL.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IRepository<Role> Roles { get; }
    IRepository<UserRole> UserRoles { get; }
    IRepository<Genre> Genres { get; }
    IMovieRepository Movies { get; }
    IRepository<Auditorium> Auditoriums { get; }
    IRepository<SeatType> SeatTypes { get; }
    IRepository<Seat> Seats { get; }
    IShowtimeRepository Showtimes { get; }
    IBookingRepository Bookings { get; }
    ITicketRepository Tickets { get; }
    IRepository<Invoice> Invoices { get; }
    IRepository<Payment> Payments { get; }
    IRepository<AuditLog> AuditLogs { get; }
    IRepository<UserRefreshToken> UserRefreshTokens { get; }
    IRepository<PasswordResetToken> PasswordResetTokens { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

