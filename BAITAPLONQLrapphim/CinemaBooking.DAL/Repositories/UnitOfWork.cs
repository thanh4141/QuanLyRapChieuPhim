using Microsoft.EntityFrameworkCore.Storage;

namespace CinemaBooking.DAL.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly CinemaDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(CinemaDbContext context)
    {
        _context = context;
        Users = new UserRepository(context);
        Roles = new Repository<Entities.Role>(context);
        UserRoles = new Repository<Entities.UserRole>(context);
        Genres = new Repository<Entities.Genre>(context);
        Movies = new MovieRepository(context);
        Auditoriums = new Repository<Entities.Auditorium>(context);
        SeatTypes = new Repository<Entities.SeatType>(context);
        Seats = new Repository<Entities.Seat>(context);
        Showtimes = new ShowtimeRepository(context);
        Bookings = new BookingRepository(context);
        Tickets = new TicketRepository(context);
        Invoices = new Repository<Entities.Invoice>(context);
        Payments = new Repository<Entities.Payment>(context);
        AuditLogs = new Repository<Entities.AuditLog>(context);
        UserRefreshTokens = new Repository<Entities.UserRefreshToken>(context);
        PasswordResetTokens = new Repository<Entities.PasswordResetToken>(context);
    }

    public IUserRepository Users { get; }
    public IRepository<Entities.Role> Roles { get; }
    public IRepository<Entities.UserRole> UserRoles { get; }
    public IRepository<Entities.Genre> Genres { get; }
    public IMovieRepository Movies { get; }
    public IRepository<Entities.Auditorium> Auditoriums { get; }
    public IRepository<Entities.SeatType> SeatTypes { get; }
    public IRepository<Entities.Seat> Seats { get; }
    public IShowtimeRepository Showtimes { get; }
    public IBookingRepository Bookings { get; }
    public ITicketRepository Tickets { get; }
    public IRepository<Entities.Invoice> Invoices { get; }
    public IRepository<Entities.Payment> Payments { get; }
    public IRepository<Entities.AuditLog> AuditLogs { get; }
    public IRepository<Entities.UserRefreshToken> UserRefreshTokens { get; }
    public IRepository<Entities.PasswordResetToken> PasswordResetTokens { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

